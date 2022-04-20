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

        // var a1 = -89.9f;
        // double i1 = 148.0511121026763d;
        // float f1 = 500f;
        //
        // var testImp1 = new ComplexDouble(i1 * Mathf.Cos(a1 * Mathf.Deg2Rad), i1 * Mathf.Sin(a1 * Mathf.Deg2Rad));
        // var mag1 = testImp1.Magnitude;
        //
        // float c1 = ComputeCapacitance(testImp1, f1);
        // float r1 = ComputeActiveResistanceWithCapacitance(testImp1, f1);
        //
        // var rMag1 = Mathf.Sqrt(r1 * r1 + 1f / Mathf.Pow(2f * Mathf.PI * f1 * c1, 2));
        //
        // var a2 = 89.9f;
        // double i2 = 1325.145731049113d;
        // float f2 = 507.26f;
        //
        // var testImp2 = new ComplexDouble(i2 * Mathf.Cos(a2 * Mathf.Deg2Rad), i2 * Mathf.Sin(a2 * Mathf.Deg2Rad));
        // var mag2 = testImp2.Magnitude;
        //
        // float l2 = ComputeInductance(testImp2, f2);
        // float r2 = ComputeActiveResistanceWithInductance(testImp2, f2);
        //
        // var rMag2 = Mathf.Sqrt(r2 * r2 + Mathf.Pow(2f * Mathf.PI * f2 * l2, 2));
    }

    private void OnDestroy()
    {
        waveformGenerator.OnSamplesChunkReady -= OnWaveformSamplesChunkReady;
        microphoneListener.OnSamplesChunkReady -= OnMicrophoneListenerSamplesChunkReady;
    }

    public ComplexDouble ComputeImpedance(int samplePosition)
    {
        Assert.IsTrue(samplePosition >= 0 && samplePosition < _inputSpectrum.Length);
        return equivalenceResistance * _inputSpectrum[samplePosition] / (_outputSpectrum[samplePosition] - _inputSpectrum[samplePosition]);
    }

    public float ComputeActiveResistanceWithCapacitance(ComplexDouble impedance, float frequency)
    {
        float impedanceMagnitude = (float) impedance.Magnitude;
        float angularFrequency = GetAngularFrequencyFor(frequency);
        
        return Mathf.Sqrt(Mathf.Abs(impedanceMagnitude * impedanceMagnitude - 1f / Mathf.Pow(angularFrequency * ComputeCapacitance(impedance, frequency), 2)));
    }
    
    public float ComputeActiveResistanceWithInductance(ComplexDouble impedance, float frequency)
    {
        float impedanceMagnitude = (float) impedance.Magnitude;
        float angularFrequency = GetAngularFrequencyFor(frequency);
        
        return Mathf.Sqrt(impedanceMagnitude * impedanceMagnitude - Mathf.Pow(angularFrequency * ComputeInductance(impedance, frequency), 2));
    }
    
    public float ComputeCapacitance(ComplexDouble impedance, float frequency)
    {
        float impedanceMagnitude = (float) impedance.Magnitude;
        float angularFrequency = GetAngularFrequencyFor(frequency);
        
        float clampedAngleInDeg = Mathf.Clamp((float) impedance.AngleInDeg, -89.9f, 89.9f);
        float tanImpedancePhase = Mathf.Tan(clampedAngleInDeg * Mathf.Deg2Rad);

        return Mathf.Sqrt(Mathf.Abs(tanImpedancePhase * tanImpedancePhase - 1f)) / (angularFrequency * Mathf.Max(Mathf.Abs(tanImpedancePhase), 0.01f) * impedanceMagnitude);
    }

    public float ComputeInductance(ComplexDouble impedance, float frequency)
    {
        float impedanceMagnitude = (float) impedance.Magnitude;
        float angularFrequency = GetAngularFrequencyFor(frequency);
        
        float clampedAngleInDeg = Mathf.Clamp((float) impedance.AngleInDeg, -89.9f, 89.9f);
        float tanImpedancePhase = Mathf.Tan(clampedAngleInDeg * Mathf.Deg2Rad);
        
        return impedanceMagnitude * tanImpedancePhase / (angularFrequency * Mathf.Sqrt(1f + tanImpedancePhase * tanImpedancePhase));
    }

    private float GetAngularFrequencyFor(float frequency) => 2f * Mathf.PI * frequency;
    
    private void OnWaveformSamplesChunkReady(float[] samplesChunk)
    {
        _outputSpectrum = FFT.ForwardTransform(ComplexDouble.FloatToComplex(samplesChunk));
    }
    
    private void OnMicrophoneListenerSamplesChunkReady(float[] samplesChunk)
    {
        _inputSpectrum = FFT.ForwardTransform(ComplexDouble.FloatToComplex(samplesChunk));
    }
}
