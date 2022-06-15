using System;
using UnityEngine;

/// <summary>
/// Расширения для обработки дискретных сигналов.
/// </summary>
public static class SignalProcessingExtensions
{
    const float DefaultRefLevel = 0.7071f; // Full scale sin wave = 0 dBFS : refLevel = 1 / sqrt(2)
    const float LevelZeroOffset = 1.5849e-13f;

    /// <summary>
    /// Возвращает пиковое значение в спектре сигнала.
    /// </summary>
    /// <param name="values">Сигнал.</param>
    /// <param name="frequency">Несущая частота.</param>
    /// <param name="samplingRate">Частота дискретизации сигнала.</param>
    /// <returns></returns>
    public static ComplexFloat ComplexPeak(this ReadOnlySpan<float> values, float frequency, float samplingRate)
    {
        const int fftSize = 1024;
        
        ComplexFloat[] complexValues = ComplexFloat.FloatToComplex(values, fftSize);
        ComplexFloat[] spectrum = FFT.ForwardTransform(complexValues);
        
        int peakIndex = Mathf.RoundToInt(frequency / samplingRate * fftSize);
        return spectrum[peakIndex];
    }
    
    /// <summary>
    /// Возвращает пиковое значение переданного сигнала.
    /// </summary>
    /// <param name="values">Сигнал.</param>
    /// <returns></returns>
    public static float Peak(this ReadOnlySpan<float> values) => Mathf.Sqrt(2) * values.Rms();
    
    /// <summary>
    /// Возвращает среднеквадратичное значение переданного сигнала.
    /// </summary>
    /// <param name="values">Сигнал.</param>
    /// <returns></returns>
    public static float Rms(this ReadOnlySpan<float> values)
    {
        float squareSum = 0f;

        for (int i = 0; i < values.Length; i++)
        {
            squareSum += values[i] * values[i];
        }

        return Mathf.Sqrt(squareSum / values.Length);
    }

    /// <summary>
    /// Возвращает среднеквадратичное значение в относительном логарифмическом масштабе dBFS (dB relative to full scale).
    /// </summary>
    /// <param name="rmsValue">Среднеквадратичное значение.</param>
    /// <param name="refLevel">Опорное значение, определяющее значение rms, которому будет соответствовать уровень 0 dBFS.</param>
    /// <returns></returns>
    public static float Level(this float rmsValue, float refLevel = DefaultRefLevel) =>
        20f * Mathf.Log10(rmsValue / refLevel + LevelZeroOffset);

    /// <summary>
    /// Возвращает среднеквадратичное значение из его логарифмического представления.
    /// </summary>
    /// <param name="levelValue">Среднеквадратичное значение в относительном логарифмическом масштабе dBFS.</param>
    /// <param name="refLevel">Опорное значение, определяющее значение rms, которому будет соответствовать уровень 0 dBFS.</param>
    /// <returns></returns>
    public static float InverseLevel(this float levelValue, float refLevel = DefaultRefLevel) =>
        Mathf.Pow(10f, levelValue / 20f) * refLevel - LevelZeroOffset;
}
