using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ImpedanceMeasurer : MonoBehaviour
{
    private const float OctaveFactor = 0.7032f;
    
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
        StartCoroutine(StartMeasureProcess());
    }

    private IEnumerator StartMeasureProcess()
    {
        CurrentFrequency = lowCutOffFrequency;
        _octaveScaler = OctaveFactor * (1f / (int) frequencyIncrement);
        
        do
        {
            if (waveformGenerator.IsGenerating)
            {
                waveformGenerator.StopGeneration();
            }

            if (microphoneListener.IsListening)
            {
                microphoneListener.StopListening();
            }
            
            waveformGenerator.StartGeneration(CurrentFrequency, samplingRate);
            microphoneListener.StartListening(samplingRate);

            yield return new WaitForSecondsRealtime(transientTimeInMs / 1000f);

            double impedanceMagnitude = impedanceComputer.ComputeImpedance(0).Magnitude;
            float capacitance = impedanceComputer.ComputeCapacitance(0);

            Debug.Log($"impedance: {impedanceMagnitude} | capacitance: {capacitance}");
            CurrentFrequency += CurrentFrequency * _octaveScaler;
        }
        while (CurrentFrequency < highCutOffFrequency);
        
        waveformGenerator.StopGeneration();
        microphoneListener.StopListening();
    }
}
