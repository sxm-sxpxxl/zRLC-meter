using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ImpedanceMeasurer : MonoBehaviour
{
    private const float OctaveFactor = 0.7032f;

    public event Action OnImpedanceMeasuringStarted = delegate { };
    public event Action OnImpedanceMeasuringFinished = delegate { };
    public event Action<ComplexDouble, float> OnImpedanceMeasured = delegate { };

        [Header("Dependencies")]
    [SerializeField] private WaveformGenerator waveformGenerator;
    [SerializeField] private MicrophoneListener microphoneListener;
    [SerializeField] private ImpedanceComputer impedanceComputer;

    [Header("Settings")]
    [SerializeField, Min(0f)] private float lowCutOffFrequency = 20f;
    [SerializeField, Min(0f)] private float highCutOffFrequency = 2000f;
    [SerializeField] private FrequencyIncrement frequencyIncrement = FrequencyIncrement.OneTwentyFourthOctave;
    [SerializeField] private SamplingRatePreset samplingRate = SamplingRatePreset.AudioCD;

    [Space]
    [SerializeField, Range(10f, 1000f)] private float transientTimeInMs = 100f;

    private float _octaveScaler = 0f;
    private Coroutine _measuringProcessCoroutine;

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

    public void StartMeasuring()
    {
        StopMeasuring();
        
        _measuringProcessCoroutine = StartCoroutine(StartMeasureProcess());
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

    private IEnumerator StartMeasureProcess()
    {
        float previousFrequency;
        CurrentFrequency = lowCutOffFrequency;
        _octaveScaler = OctaveFactor * (1f / (int) frequencyIncrement);

        do
        {
            previousFrequency = CurrentFrequency;
            
            if (waveformGenerator.IsGenerating && microphoneListener.IsListening)
            {
                StopGenerationAndListening();
            }

            waveformGenerator.StartGeneration(CurrentFrequency, samplingRate);
            microphoneListener.StartListening(samplingRate);

            yield return new WaitForSecondsRealtime(transientTimeInMs / 1000f);

            ComplexDouble impedance = impedanceComputer.ComputeImpedance(0);
            Debug.Log($"|Z| = {impedance.Magnitude}");
            OnImpedanceMeasured.Invoke(impedance, CurrentFrequency);
            
            CurrentFrequency += CurrentFrequency * _octaveScaler;
        }
        while (CurrentFrequency < highCutOffFrequency + previousFrequency * _octaveScaler);

        CurrentFrequency = previousFrequency;
        StopGenerationAndListening();
        
        OnImpedanceMeasuringFinished.Invoke();
    }

    private void StopGenerationAndListening()
    {
        waveformGenerator.StopGeneration();
        microphoneListener.StopListening();
    }
}
