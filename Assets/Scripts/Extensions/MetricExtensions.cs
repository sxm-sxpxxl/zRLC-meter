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
    
    public static float ConvertToNormal(this float value, Metric fromMetric)
    {
        return ConvertTo(value, fromMetric, Metric.Normal);
    }
    
    public static float ConvertTo(this float value, Metric fromMetric, Metric toMetric)
    {
        int metricDifference = (int) fromMetric - (int) toMetric;
        return value * Mathf.Pow(MetricStep, metricDifference);
    }

    public static (float, string) PrefixConvertedValue(this float value)
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
