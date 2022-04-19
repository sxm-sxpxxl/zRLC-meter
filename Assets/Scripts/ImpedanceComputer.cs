using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class ImpedanceComputer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private WaveformGenerator waveformGenerator;
    [SerializeField] private MicrophoneListener microphoneListener;

    [Header("Settings")]
    [SerializeField, Min(0f), Tooltip("Equivalence resistance in Ohm")] private float equivalenceResistance = 100;

    private ComplexDouble[] _outputSpectrum, _inputSpectrum;
    
    private void Awake()
    {
        waveformGenerator.OnSamplesChunkReady += OnWaveformSamplesChunkReady;
        microphoneListener.OnSamplesChunkReady += OnMicrophoneListenerSamplesChunkReady;
    }

    private void OnDestroy()
    {
        waveformGenerator.OnSamplesChunkReady -= OnWaveformSamplesChunkReady;
        microphoneListener.OnSamplesChunkReady -= OnMicrophoneListenerSamplesChunkReady;
    }

    private void OnWaveformSamplesChunkReady(float[] samplesChunk)
    {
        _outputSpectrum = FFT.ForwardTransform(ComplexDouble.FloatToComplex(samplesChunk));
    }
    
    private void OnMicrophoneListenerSamplesChunkReady(float[] samplesChunk)
    {
        _inputSpectrum = FFT.ForwardTransform(ComplexDouble.FloatToComplex(samplesChunk));
    }

    private ComplexDouble ComputeImpedance(int samplePosition)
    {
        Assert.IsTrue(samplePosition >= 0 && samplePosition < _inputSpectrum.Length);
        return equivalenceResistance * _inputSpectrum[samplePosition] / (_outputSpectrum[samplePosition] - _inputSpectrum[samplePosition]);
    }

    private float ComputeCapacitance(int samplePosition)
    {
        Assert.IsTrue(samplePosition >= 0 && samplePosition < _inputSpectrum.Length);
        
        ComplexDouble impedance = ComputeImpedance(samplePosition);
        float impedanceMagnitude = (float) impedance.Magnitude;
        float tanImpedancePhase = Mathf.Tan((float) impedance.AngleInRad);
        float angularFrequency = 2f * Mathf.PI * waveformGenerator.WaveFrequency;
        
        return (1f + tanImpedancePhase * tanImpedancePhase) / Mathf.Pow(impedanceMagnitude * angularFrequency * tanImpedancePhase, 2);
    }
}
