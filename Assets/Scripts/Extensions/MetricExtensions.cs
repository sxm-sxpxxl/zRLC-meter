using System.Collections.Generic;
using UnityEngine;

public enum Metric
{
    Nano   = -3,
    Micro  = -2,
    Milli  = -1,
    Normal = 0,
    Kilo   = +1,
    Mega   = +2,
    Giga   = +3
}

/// <summary>
/// Расширения для преобразования значений из одной метрики в другую. Например, 0.1с -> 100мс; 1000 Гц -> 1кГЦ.
/// </summary>
public static class MetricExtensions
{
    private const float MetricStep = 1e+03f;

    private static readonly Dictionary<Metric, string> PrefixMetricMap = new Dictionary<Metric, string>
    {
        { Metric.Nano,   "n" },
        { Metric.Micro,  "μ" },
        { Metric.Milli,  "m" },
        { Metric.Normal, ""  },
        { Metric.Kilo,   "k" },
        { Metric.Mega,   "M" },
        { Metric.Giga,   "G" },
    };
    
    /// <summary>
    /// Преобразовать значение из исходной метрики (например, значение 100 с метрикой 'm' понимается как 100m)
    /// в нормальную (т.е. 100m -> 0.1).
    /// </summary>
    /// <param name="value">значение в исходной метрике.</param>
    /// <param name="fromMetric">исходная метрика.</param>
    /// <returns>значение в нормальной метрике.</returns>
    public static float ConvertToNormal(this float value, Metric fromMetric)
    {
        return ConvertTo(value, fromMetric, Metric.Normal);
    }
    
    /// <summary>
    /// Преобразовать значение из исходной метрики в итоговую. (например, 1m -> 1000u).
    /// </summary>
    /// <param name="value">значение в исходной метрике.</param>
    /// <param name="fromMetric">исходная метрика.</param>
    /// <param name="toMetric">итоговая метрика.</param>
    /// <returns>значение в итоговой метрике.</returns>
    public static float ConvertTo(this float value, Metric fromMetric, Metric toMetric)
    {
        int metricDifference = (int) fromMetric - (int) toMetric;
        return value * Mathf.Pow(MetricStep, metricDifference);
    }

    /// <summary>
    /// Автоматически определить наиболее ближайшую к значению метрику, преобразовать его в нее
    /// и вместе со значением вернуть соответствующий метрике префикс. 
    /// </summary>
    /// <param name="value">значение без заданной метрики.</param>
    /// <returns>пара (значение в ближайшей метрике, префикс ближайшей метрики).</returns>
    public static (float, string) AutoConvertNormalValue(this float value)
    {
        Metric desiredMetric = GetDesiredMetric(value);

        float convertedValue = ConvertTo(value, Metric.Normal, desiredMetric);
        string prefix = PrefixMetricMap[desiredMetric];
        
        return (convertedValue, prefix);
    }

    private static Metric GetDesiredMetric(this float value)
    {
        int nearestMetricNumber = Mathf.FloorToInt(Mathf.Log(value, MetricStep));
        return (Metric) Mathf.Clamp(nearestMetricNumber, (int) Metric.Nano, (int) Metric.Giga);
    }
}
