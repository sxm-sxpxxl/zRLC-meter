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
    public event Action OnImpedanceMeasuringFinished = delegate { };
    public event Action<ImpedanceMeasureData> OnImpedanceMeasured = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;
    [SerializeField] private ChannelsCalibrator channelsCalibrator;

    [Header("Settings")]
    [SerializeField, Range(1, 100)] private int iterationsNumber = 10;
    [SerializeField] private FrequencyIncrement frequencyIncrement = FrequencyIncrement.OneTwentyFourthOctave;

    private float _octaveScaler;
    private Coroutine _measuringProcessCoroutine;
    
    private OutputDeviceGenerator _outputDeviceGenerator;
    private InputDeviceListener _inputDeviceListener;

    [field: Header("Debug Info"), SerializeField]
    private float CurrentFrequency { get; set; }

    private enum FrequencyIncrement
    {
        [InspectorName("1 ⁒ 24 octave")]
        OneTwentyFourthOctave = 24,
        [InspectorName("1 ⁒ 48 octave")]
        OneFortyEighthOctave = 48
    }
    
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
        float previousFrequency;
        CurrentFrequency = generalSettings.LowCutOffFrequency;
        _octaveScaler = OctaveFactor * (1f / (int) frequencyIncrement);

        do
        {
            previousFrequency = CurrentFrequency;
            
            _outputDeviceGenerator.StartGeneration(
                generalSettings.OutputDeviceIndex,
                CurrentFrequency,
                generalSettings.SampleRate
            );
            _inputDeviceListener.StartListening(
                generalSettings.InputDeviceIndex,
                generalSettings.InputOutputChannelOffsets
            );
            
            yield return new WaitForSecondsRealtime(generalSettings.TransientTimeInMs.ConvertToNormal(fromMetric: Metric.Milli));
            
            float impedanceMagnitude = 0f, impedancePhaseInDeg = 0f;
            for (int i = 0; i < iterationsNumber;)
            {
                float computedImpedanceMagnitude = ZRLCHelper.ComputeImpedanceMagnitude(
                    _inputDeviceListener.InputFilledDataSamples,
                    _inputDeviceListener.OutputFilledDataSamples,
                    generalSettings.EquivalenceResistance,
                    channelsCalibrator.CalibrationMagnitudeRatioRms
                );
                
                float computedImpedancePhaseInDeg = ZRLCHelper.ComputeImpedancePhaseInDeg(
                    _inputDeviceListener.InputFilledDataSamples,
                    _inputDeviceListener.OutputFilledDataSamples,
                    _inputDeviceListener.SampleRate,
                    CurrentFrequency,
                    channelsCalibrator.CalibrationMagnitudeRatioRms
                );

                if (float.IsNaN(computedImpedanceMagnitude) || float.IsNaN(computedImpedancePhaseInDeg))
                {
                    yield return null;
                    continue;
                }

                impedanceMagnitude += computedImpedanceMagnitude;
                impedancePhaseInDeg += computedImpedancePhaseInDeg;
                i++;
                
                yield return null;
            }
            impedanceMagnitude /= iterationsNumber;
            impedancePhaseInDeg /= iterationsNumber;

            Debug.Log($"f: <color=yellow>{CurrentFrequency} Hz</color>  " +
                      $"|Z|: <color=green>{impedanceMagnitude} Ohm</color>  " +
                      $"φ: <color=red>{impedancePhaseInDeg}°</color>");

            OnImpedanceMeasured.Invoke(new ImpedanceMeasureData
            {
                magnitude = impedanceMagnitude,
                phaseInDeg = impedancePhaseInDeg,
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
