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
        float phaseInDeg = ComputePhaseShift(inputSignalSamples, outputSignalSamples, sampleRate, frequency, calibrationMagnitudeRatioRms);
        return phaseInDeg;
    }

    public static float ComputePhaseShift(
        ReadOnlySpan<float> inputSignalSamples,
        ReadOnlySpan<float> outputSignalSamples,
        int sampleRate,
        float frequency,
        float calibrationMagnitudeRatioRms
    )
    {
        float interval = 1f / frequency;

        float iRms = inputSignalSamples.Rms();
        float oRms = outputSignalSamples.Rms();
        
        float averageSignalsProduct = 0f;
        for (int i = 0; i < inputSignalSamples.Length; i++)
        {
            averageSignalsProduct += inputSignalSamples[i] * outputSignalSamples[i];
        }
        averageSignalsProduct /= inputSignalSamples.Length;

        NativeArray<float> product = new NativeArray<float>(inputSignalSamples.Length, Allocator.Temp);

        for (int i = 0; i < inputSignalSamples.Length; i++)
        {
            product[i] = inputSignalSamples[i] * outputSignalSamples[i];
        }

        float rms = product.GetReadOnlySpan().Rms();

        float pick = Mathf.Max(inputSignalSamples.ToArray());
        float phaseInDeg = Mathf.Acos(2f * calibrationMagnitudeRatioRms * iRms * oRms / (pick * pick)) * Mathf.Rad2Deg;
        
        return phaseInDeg;
    }

    private static float GetAverageSamplesLengthBeforeSignChange(ReadOnlySpan<float> samples, int sampleRate, float frequency)
    {
        if (samples.Length == 0)
        {
            return 0;
        }
        
        int samplesPerPeriod = Mathf.CeilToInt(sampleRate / frequency);
        int previousSampleIndex = 0;

        var phaseSamplesList = new List<int>(capacity: Mathf.CeilToInt((float)samples.Length / samplesPerPeriod));
        float initialSign = Mathf.Sign(samples[0]), currentSign;
        
        for (int currentSampleIndex = previousSampleIndex; currentSampleIndex < samples.Length; currentSampleIndex++)
        {
            currentSign = Mathf.Sign(samples[currentSampleIndex]);

            if (initialSign * currentSign < 0f)
            {
                phaseSamplesList.Add(currentSampleIndex - previousSampleIndex);
                int nextSampleIndex = previousSampleIndex + samplesPerPeriod;
                
                currentSampleIndex = Mathf.Clamp(nextSampleIndex, 0, samples.Length);
                previousSampleIndex = currentSampleIndex;
                
                // Debug.Log($"{phaseSamplesList[^1]}");
            }
        }

        float averagePhaseSampleLength = 0f;
        for (int i = 0; i < phaseSamplesList.Count; i++)
        {
            averagePhaseSampleLength += phaseSamplesList[i];
        }
        averagePhaseSampleLength /= phaseSamplesList.Count;

        // Debug.Log($"averagePhaseSample: {averagePhaseSampleLength}");
        return averagePhaseSampleLength;
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
