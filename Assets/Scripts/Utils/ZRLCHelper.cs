using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public static class ZRLCHelper
{
    public static float ComputeImpedanceMagnitude(
        ReadOnlySpan<float> inputSignalSamples,
        ReadOnlySpan<float> outputSignalSamples,
        float equivalenceResistance,
        float calibrationMagnitudeRatioRms
    )
    {
        var inputRms = inputSignalSamples.Rms();
        var outputRms = outputSignalSamples.Rms();
        
        var calibratedIORatio = calibrationMagnitudeRatioRms * (inputRms / outputRms);
        var impedanceMagnitude = equivalenceResistance / (calibratedIORatio - 1f);
        
        return impedanceMagnitude;
    }

    public static float ComputeImpedancePhaseInDeg(
        ReadOnlySpan<float> inputSignalSamples,
        ReadOnlySpan<float> outputSignalSamples,
        int sampleRate,
        float frequency,
        float calibrationMagnitudeRatioRms
    )
    {
        int maxSamplesLength = Mathf.Clamp(Mathf.CeilToInt(sampleRate / frequency), 0, inputSignalSamples.Length);
        float averageSignalsProduct = 0f;

        for (int i = 0; i < maxSamplesLength; i++)
        {
            averageSignalsProduct += inputSignalSamples[i] * outputSignalSamples[i];
        }
        
        averageSignalsProduct /= maxSamplesLength;

        float inputPeak = inputSignalSamples.Peak();
        float outputPeak = outputSignalSamples.Peak();

        float phaseInDeg = -Mathf.Acos(2f * calibrationMagnitudeRatioRms * averageSignalsProduct / (inputPeak * outputPeak)) * Mathf.Rad2Deg;
        return phaseInDeg;
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
