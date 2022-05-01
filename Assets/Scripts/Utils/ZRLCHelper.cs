using System;
using UnityEngine;

public static class ZRLCHelper
{
    public static float ComputeImpedanceMagnitude(
        ReadOnlySpan<float> inputSignalSamples,
        ReadOnlySpan<float> outputSignalSamples,
        float equivalenceResistance,
        float calibrationRatioRms
    )
    {
        var inputRms = inputSignalSamples.Rms();
        var outputRms = outputSignalSamples.Rms();
        
        var calibratedIORatio = calibrationRatioRms * (inputRms / outputRms);
        var impedanceMagnitude = equivalenceResistance / (calibratedIORatio - 1f);
        
        return impedanceMagnitude;
    }
    
    public static float ComputeActiveResistanceWithCapacitance(float impedanceMagnitude, float impedancePhaseInDeg, float frequency)
    {
        float angularFrequency = GetAngularFrequencyFor(frequency);
        float capacitance = ComputeCapacitance(impedanceMagnitude, impedancePhaseInDeg, frequency);
        
        return Mathf.Sqrt(Mathf.Abs(impedanceMagnitude * impedanceMagnitude - 1f / Mathf.Pow(angularFrequency * capacitance, 2)));
    }
    
    public static float ComputeActiveResistanceWithInductance(float impedanceMagnitude, float impedancePhaseInDeg, float frequency)
    {
        float angularFrequency = GetAngularFrequencyFor(frequency);
        float inductance = ComputeInductance(impedanceMagnitude, impedancePhaseInDeg, frequency);
        
        return Mathf.Sqrt(impedanceMagnitude * impedanceMagnitude - Mathf.Pow(angularFrequency * inductance, 2));
    }
    
    public static float ComputeCapacitance(float impedanceMagnitude, float impedancePhaseInDeg, float frequency)
    {
        float angularFrequency = GetAngularFrequencyFor(frequency);
        
        float clampedPhaseInDeg = Mathf.Clamp(impedancePhaseInDeg, -89.9f, 89.9f);
        float tanImpedancePhase = Mathf.Tan(clampedPhaseInDeg * Mathf.Deg2Rad);

        return Mathf.Sqrt(Mathf.Abs(tanImpedancePhase * tanImpedancePhase - 1f)) / (angularFrequency * Mathf.Max(Mathf.Abs(tanImpedancePhase), 0.01f) * impedanceMagnitude);
    }

    public static float ComputeInductance(float impedanceMagnitude, float impedancePhaseInDeg, float frequency)
    {
        float angularFrequency = GetAngularFrequencyFor(frequency);
        
        float clampedPhaseInDeg = Mathf.Clamp(impedancePhaseInDeg, -89.9f, 89.9f);
        float tanImpedancePhase = Mathf.Tan(clampedPhaseInDeg * Mathf.Deg2Rad);
        
        return impedanceMagnitude * tanImpedancePhase / (angularFrequency * Mathf.Sqrt(1f + tanImpedancePhase * tanImpedancePhase));
    }

    private static float GetAngularFrequencyFor(float frequency) => 2f * Mathf.PI * frequency;
}
