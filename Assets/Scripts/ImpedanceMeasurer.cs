using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class ImpedanceMeasurer : MonoBehaviour
{
    private const float OctaveFactor = 0.7032f;

    public event Action OnImpedanceMeasuringStarted = delegate { };
    public event Action OnImpedanceMeasuringFinished = delegate { };
    public event Action<float, float> OnImpedanceMeasured = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;
    [SerializeField] private ChannelsCalibrator channelsCalibrator;

    [Header("Settings")]
    [SerializeField, Range(1, 20)] private int iterationsNumber = 10;
    [SerializeField, Min(0f)] private float lowCutOffFrequency = 20f;
    [SerializeField, Min(0f)] private float highCutOffFrequency = 2000f;
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
    
    private void OnValidate()
    {
        if (lowCutOffFrequency > highCutOffFrequency)
        {
            Debug.LogError("Low cut-off frequency is smaller than High cut-off frequency!");
        }
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
        CurrentFrequency = lowCutOffFrequency;
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
            
            float impedanceMagnitude = 0f;
            for (int i = 0; i < iterationsNumber;)
            {
                float computedImpedanceMagnitude = ZRLCHelper.ComputeImpedanceMagnitude(
                    _inputDeviceListener.InputFilledDataSamples,
                    _inputDeviceListener.OutputFilledDataSamples,
                    generalSettings.EquivalenceResistance,
                    channelsCalibrator.CalibrationRatioRms
                );

                if (float.IsNaN(computedImpedanceMagnitude))
                {
                    yield return null;
                    continue;
                }

                impedanceMagnitude += computedImpedanceMagnitude;
                i++;
                
                yield return null;
            }
            impedanceMagnitude /= iterationsNumber;

            Debug.Log($"<color=green>|Z| = {impedanceMagnitude}</color> | <color=red>phase: -</color>");
            OnImpedanceMeasured.Invoke(impedanceMagnitude, CurrentFrequency);
            
            StopGenerationAndListening();
            
            CurrentFrequency += CurrentFrequency * _octaveScaler;
        }
        while (CurrentFrequency < highCutOffFrequency + previousFrequency * _octaveScaler);

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
