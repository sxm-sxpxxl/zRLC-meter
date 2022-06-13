using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Проводит процесс измерений в диапазоне частот [lowCutOffFreq, highCutOffFreq] и уведомляет о ходе процесса подписчиков.
/// </summary>
[DisallowMultipleComponent]
public sealed class ImpedanceMeasurer : MonoBehaviour
{
    private const float OctaveFactor = 0.7032f;

    public event Action OnImpedanceMeasuringStarted = delegate { };
    public event Action<string> OnImpedanceMeasuringErrorOccurred = delegate { };
    public event Action OnImpedanceMeasuringFinished = delegate { };
    public event Action<ImpedanceMeasureData> OnImpedanceMeasured = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;
    [SerializeField] private ChannelsCalibrator channelsCalibrator;

    private float _octaveScaler;
    private Coroutine _measuringProcessCoroutine;
    
    private OutputDeviceGenerator _outputDeviceGenerator;
    private InputDeviceListener _inputDeviceListener;

    [field: Header("Debug Info"), SerializeField]
    private float CurrentFrequency { get; set; }
    
    private bool IsChannelCountValid => _inputDeviceListener.GetChannelCountBy(generalSettings.InputDeviceIndex) == 2;

    private void Start()
    {
        _outputDeviceGenerator = OutputDeviceGenerator.Instance;
        _inputDeviceListener = InputDeviceListener.Instance;

        Assert.IsNotNull(_outputDeviceGenerator);
        Assert.IsNotNull(_inputDeviceListener);
    }

    public void StartMeasuring()
    {
        StopMeasuring();
        
        _measuringProcessCoroutine = StartCoroutine(MeasuringCoroutine());
        OnImpedanceMeasuringStarted.Invoke();
    }

    public void StopMeasuring()
    {
        if (_measuringProcessCoroutine != null)
        {
            StopCoroutine(_measuringProcessCoroutine);
            StopGenerationAndListening();
        }
    }

    private IEnumerator MeasuringCoroutine()
    {
        if (IsChannelCountValid == false)
        {
            OnImpedanceMeasuringErrorOccurred.Invoke("There must be two channels of LineIn to carry out measurements. Сheck your connections and try again.");
            yield break;
        }
        
        float previousFrequency, elapsedRetryTimeoutInSec = 0f;
        CurrentFrequency = generalSettings.LowCutOffFrequency;
        _octaveScaler = OctaveFactor * (1f / (int) generalSettings.FrequencyIncrement);

        do
        {
            previousFrequency = CurrentFrequency;
            
            _outputDeviceGenerator.StartGeneration(
                generalSettings.OutputDeviceIndex,
                CurrentFrequency,
                generalSettings.SamplingRate
            );
            _inputDeviceListener.StartListening(
                generalSettings.InputDeviceIndex,
                generalSettings.InputOutputChannelOffsets
            );
            
            yield return new WaitForSecondsRealtime(generalSettings.TransientTimeInMs.ConvertToNormal(fromMetric: Metric.Milli));

            ComplexFloat testImpedance = ComplexFloat.Zero;
            float inputChannelRms = 0f, outputChannelRms = 0f;
            
            for (int i = 0; i < generalSettings.AveragingIterations;)
            {
                if (_inputDeviceListener.TryGetAndReleaseFilledSamplesByIntervals(
                    CurrentFrequency,
                    generalSettings.SignalIntervalsCount,
                    out ReadOnlySpan<float> inputDataSamples,
                    out ReadOnlySpan<float> outputDataSamples
                ) == false)
                {
                    yield return null;
                    continue;
                }

                ComplexFloat computedImpedance = ZRLCHelper.ComputeTestImpedance(
                    inputDataSamples,
                    outputDataSamples,
                    generalSettings.EquivalenceResistance,
                    channelsCalibrator.LineInputImpedance,
                    channelsCalibrator.GroundImpedance,
                    CurrentFrequency,
                    generalSettings.SamplingRate
                );

                if (float.IsNaN(computedImpedance.Magnitude) || float.IsNaN(computedImpedance.AngleInRad))
                {
                    if (elapsedRetryTimeoutInSec > generalSettings.RetryTimeoutInSec)
                    {
                        StopGenerationAndListening();
                        OnImpedanceMeasuringErrorOccurred.Invoke("Impedance is measured as NaN. Сheck your circuit and try again.");
                        yield break;
                    }
                    
                    elapsedRetryTimeoutInSec += Time.deltaTime;
                    yield return null;
                    continue;
                }

                inputChannelRms += inputDataSamples.Rms();
                outputChannelRms += outputDataSamples.Rms();
                
                testImpedance += computedImpedance;
                i++;

                elapsedRetryTimeoutInSec = 0f;
                yield return null;
            }
            
            inputChannelRms /= generalSettings.AveragingIterations;
            outputChannelRms /= generalSettings.AveragingIterations;
            Debug.Log($"<color=yellow>Input channel RMS: </color> {inputChannelRms} V  " +
                      $"<color=yellow>Output channel RMS: </color> {outputChannelRms} V");
            
            testImpedance /= generalSettings.AveragingIterations;
            Debug.Log($"f: <color=yellow>{CurrentFrequency} Hz</color>  " +
                      $"|Z|: <color=green>{testImpedance.Magnitude} Ohm</color>  " +
                      $"φ: <color=red>{testImpedance.AngleInRad * Mathf.Rad2Deg}°</color>");

            OnImpedanceMeasured.Invoke(new ImpedanceMeasureData
            {
                impedance = testImpedance,
                frequency = CurrentFrequency
            });
            
            StopGenerationAndListening();
            
            CurrentFrequency += CurrentFrequency * _octaveScaler;
        }
        while (CurrentFrequency < generalSettings.HighCutOffFrequency + previousFrequency * _octaveScaler);

        CurrentFrequency = previousFrequency;
        StopGenerationAndListening();
        
        OnImpedanceMeasuringFinished.Invoke();
    }

    private void StopGenerationAndListening()
    {
        _outputDeviceGenerator.StopGeneration();
        _inputDeviceListener.StopListening();
    }
}
