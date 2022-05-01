using System;
using UnityEngine;

public static class SignalProcessingExtensions
{
    // Full scale sin wave = 0 dBFS : refLevel = 1 / sqrt(2)
    const float DefaultRefLevel = 0.7071f;
    const float LevelZeroOffset = 1.5849e-13f;
    
    /// <summary>
    /// Return root mean square value of float span values.
    /// </summary>
    /// <param name="values">The float span values.</param>
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
    /// Return RMS value in dBFS.
    /// </summary>
    /// <param name="rmsValue">The root mean square value.</param>
    /// <param name="refLevel">The reference RMS level is assumed to be 0 dBFS.</param>
    /// <returns></returns>
    public static float Level(this float rmsValue, float refLevel = DefaultRefLevel) =>
        20f * Mathf.Log10(rmsValue / refLevel + LevelZeroOffset);

    /// <summary>
    /// Return inversed level value from dBFS.
    /// </summary>
    /// <param name="levelValue">The level value in dBFS.</param>
    /// <param name="refLevel">The reference RMS level is assumed to 0 dBFS.</param>
    /// <returns></returns>
    public static float InverseLevel(this float levelValue, float refLevel = DefaultRefLevel) =>
        Mathf.Pow(10f, levelValue / 20f) * refLevel - LevelZeroOffset;
}
