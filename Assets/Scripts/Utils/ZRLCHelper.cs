using System;
using UnityEngine;

// todo: refactoring
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
        TestComponentType testComponentType,
        float frequency,
        float samplingRate
    )
    {
        var inPeak = inputSignalSamples.ComplexPeak(frequency, samplingRate);
        var outPeak = outputSignalSamples.ComplexPeak(frequency, samplingRate);;
        
        var rref = equivalenceResistance;
        var zr = lineInputImpedance;
        var zg = groundImpedance;

        ComplexFloat testImpedance = outPeak * zr * rref / (zr * (inPeak - outPeak) - outPeak * rref) - zg;
        return testImpedance;
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
        float equivalenceResistance,
        float frequency,
        float samplingRate
    )
    {
        ComplexFloat inPeak = inputSignalSamples.ComplexPeak(frequency, samplingRate);
        ComplexFloat outPeak = outputSignalSamples.ComplexPeak(frequency, samplingRate);
        
        return equivalenceResistance / (inPeak / outPeak - 1f);
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
}
