using System;
using UnityEngine;

/// <summary>
/// Расширения для обработки дискретных сигналов.
/// </summary>
public static class SignalProcessingExtensions
{
    // Full scale sin wave = 0 dBFS : refLevel = 1 / sqrt(2)
    const float DefaultRefLevel = 0.7071f;
    const float LevelZeroOffset = 1.5849e-13f;

    /// <summary>
    /// Возвращает пиковое (или просто максимальное) значение для переданного сигнала.
    /// Отражает амплитуду переданного сигнала.
    /// </summary>
    /// <param name="values">Сигнал.</param>
    /// <returns></returns>
    public static float Peak(this ReadOnlySpan<float> values)
    {
        if (values.Length == 0)
        {
            return 0.0f;            
        }
        
        float maxValue = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > maxValue)
            {
                maxValue = values[i];
            }
        }
        
        return maxValue;
    }
    
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
