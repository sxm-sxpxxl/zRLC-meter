using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Рассчитывает импеданс (амплитуду и фазу) и RLC параметры импеданса.
/// </summary>
public static class ZRLCHelper
{
    /// <summary>
    /// Рассчитать амплитуду импеданса.
    /// </summary>
    /// <param name="inputSignalSamples">Входной сигнал.</param>
    /// <param name="outputSignalSamples">Выходной сигнал.</param>
    /// <param name="equivalenceResistance">Эквивалентное сопротивление.</param>
    /// <param name="calibrationMagnitudeRatioRms">Отношение между сигналами при калибровке.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Рассчитать фазу импеданса.
    /// </summary>
    /// <param name="inputSignalSamples">Входной сигнал.</param>
    /// <param name="outputSignalSamples">Выходной сигнал.</param>
    /// <param name="sampleRate">Частота дискретизации.</param>
    /// <param name="frequency">Частота генерируемой синусоиды.</param>
    /// <param name="calibrationMagnitudeRatioRms">Отношение между сигналами при калибровке.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Рассчитать активное сопротивление в контуре RC.
    /// </summary>
    /// <param name="data">Результат измерения импеданса.</param>
    /// <returns></returns>
    public static float ComputeActiveResistanceWithCapacitance(ImpedanceMeasureData data)
    {
        float angularFrequency = GetAngularFrequencyFor(data.frequency);
        float capacitance = ComputeCapacitance(data);
        
        return Mathf.Sqrt(Mathf.Abs(data.magnitude * data.magnitude - 1f / Mathf.Pow(angularFrequency * capacitance, 2)));
    }
    
    /// <summary>
    /// Рассчитать активное сопротивление в контуре RL.
    /// </summary>
    /// <param name="data">Результат измерения импеданса.</param>
    /// <returns></returns>
    public static float ComputeActiveResistanceWithInductance(ImpedanceMeasureData data)
    {
        float angularFrequency = GetAngularFrequencyFor(data.frequency);
        float inductance = ComputeInductance(data);
        
        return Mathf.Sqrt(data.magnitude * data.magnitude - Mathf.Pow(angularFrequency * inductance, 2));
    }
    
    /// <summary>
    /// Рассчитать емкость в контуре RC.
    /// </summary>
    /// <param name="data">Результат измерения импеданса.</param>
    /// <returns></returns>
    public static float ComputeCapacitance(ImpedanceMeasureData data)
    {
        float angularFrequency = GetAngularFrequencyFor(data.frequency);
        
        float clampedPhaseInDeg = Mathf.Clamp(data.phaseInDeg, -89.9f, 89.9f);
        float tanImpedancePhase = Mathf.Tan(clampedPhaseInDeg * Mathf.Deg2Rad);

        return Mathf.Sqrt(Mathf.Abs(tanImpedancePhase * tanImpedancePhase - 1f)) /
               (angularFrequency * Mathf.Max(Mathf.Abs(tanImpedancePhase), 0.01f) * data.magnitude);
    }

    /// <summary>
    /// Рассчитать индуктивность в контуре RL.
    /// </summary>
    /// <param name="data">Результат измерения импеданса.</param>
    /// <returns></returns>
    public static float ComputeInductance(ImpedanceMeasureData data)
    {
        float angularFrequency = GetAngularFrequencyFor(data.frequency);
        
        float clampedPhaseInDeg = Mathf.Clamp(data.phaseInDeg, -89.9f, 89.9f);
        float tanImpedancePhase = Mathf.Tan(clampedPhaseInDeg * Mathf.Deg2Rad);
        
        return data.magnitude * tanImpedancePhase /
               (angularFrequency * Mathf.Sqrt(1f + tanImpedancePhase * tanImpedancePhase));
    }

    private static float GetAngularFrequencyFor(float frequency) => 2f * Mathf.PI * frequency;
}
