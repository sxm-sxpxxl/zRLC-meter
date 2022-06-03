using System;
using UnityEngine;

/// <summary>
/// Рассчитывает импеданс (амплитуду и фазу) и RLC параметры импеданса.
/// </summary>
public static class ZRLCHelper
{
    /// <summary>
    /// Рассчитать импеданс тестового компонента с корректировкой на калибровочные данные.
    /// </summary>
    /// <param name="inputSignalSamples">Входной сигнал.</param>
    /// <param name="outputSignalSamples">Выходной сигнал.</param>
    /// <param name="equivalenceResistance">Эквивалентное сопротивление.</param>
    /// <param name="lineInputImpedance">Входной импеданс звуковой карты.</param>
    /// <param name="groundImpedance">Импеданс в цепи с тестовым компонентом.</param>
    /// <param name="testComponentType">Тип тестового компонента.</param>
    /// <returns></returns>
    public static ComplexFloat ComputeTestImpedance(
        ReadOnlySpan<float> inputSignalSamples,
        ReadOnlySpan<float> outputSignalSamples,
        float equivalenceResistance,
        ComplexFloat lineInputImpedance,
        ComplexFloat groundImpedance,
        TestComponentType testComponentType
    )
    {
        var inRms = inputSignalSamples.Rms();
        var outRms = outputSignalSamples.Rms();
        
        var rref = equivalenceResistance;
        var zr = lineInputImpedance.Magnitude;
        var zg = groundImpedance.Magnitude;

        float magnitude = outRms * zr * rref / (zr * (inRms - outRms) - outRms * rref) - zg;
        
        float phaseInRad = ComputeImpedancePhaseInRad(inputSignalSamples, outputSignalSamples);
        float sign = testComponentType == TestComponentType.Capacitance ? -1f : 1f;
        
        phaseInRad = sign * (phaseInRad + lineInputImpedance.AngleInRad);
        return ComplexFloat.FromAngle(phaseInRad, magnitude);
    }
    
    /// <summary>
    /// Рассчитать импеданс.
    /// </summary>
    /// <param name="inputSignalSamples">Входной сигнал.</param>
    /// <param name="outputSignalSamples">Выходной сигнал.</param>
    /// <param name="equivalenceResistance">Эквивалентное сопротивление.</param>
    /// <returns></returns>
    public static ComplexFloat ComputeImpedance(
        ReadOnlySpan<float> inputSignalSamples,
        ReadOnlySpan<float> outputSignalSamples,
        float equivalenceResistance
    )
    {
        float magnitude = ComputeImpedanceMagnitude(inputSignalSamples, outputSignalSamples, equivalenceResistance);
        float phaseInRad = ComputeImpedancePhaseInRad(inputSignalSamples, outputSignalSamples);

        return ComplexFloat.FromAngle(phaseInRad, magnitude);
    }

    /// <summary>
    /// Рассчитать активное сопротивление в тестовом импедансе.
    /// </summary>
    /// <param name="data">Результат измерения импеданса.</param>
    /// <returns></returns>
    public static float ComputeActiveResistance(ImpedanceMeasureData data) => data.impedance.real;
    
    /// <summary>
    /// Рассчитать емкость в контуре RC.
    /// </summary>
    /// <param name="data">Результат измерения импеданса.</param>
    /// <returns></returns>
    public static float ComputeCapacitance(ImpedanceMeasureData data) =>
        -1f / (2f * Mathf.PI * data.frequency * data.impedance.imag);

    /// <summary>
    /// Рассчитать индуктивность в контуре RL.
    /// </summary>
    /// <param name="data">Результат измерения импеданса.</param>
    /// <returns></returns>
    public static float ComputeInductance(ImpedanceMeasureData data) =>
        data.impedance.imag / (2f * Mathf.PI * data.frequency);

    private static float ComputeImpedanceMagnitude(ReadOnlySpan<float> vInSamples, ReadOnlySpan<float> vOutSamples, float rref) =>
        rref / (vInSamples.Rms() / vOutSamples.Rms() - 1f);

    private static float ComputeImpedancePhaseInRad(ReadOnlySpan<float> vInSamples, ReadOnlySpan<float> vOutSamples)
    {
        float averageProduct = 0f;
        
        for (int i = 0; i < vInSamples.Length; i++)
        {
            averageProduct += vInSamples[i] * vOutSamples[i];
        }
        
        averageProduct /= vInSamples.Length;
        
        float inPeak = vInSamples.Peak();
        float outPeak = vOutSamples.Peak();
        
        return Mathf.Acos(2f * averageProduct / (inPeak * outPeak));
    }
}
