using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MeasuringResultView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ImpedanceChartFeed impedanceChartFeed;
    [SerializeField] private ImpedanceComputer impedanceComputer;
    
    [Header("Related views")]
    [SerializeField] private Text frequencyValueText;
    [SerializeField] private Text activeResistanceValueText;
    [SerializeField] private Text capacitanceValueText;

    private readonly List<(float, string)> _matchPrefixConversionMap = new List<(float, string)>
    {
        ( 1e-09f, "n" ),
        ( 1e-06f, "μ" ),
        ( 1e-03f, "m" ),
        ( 1e+00f, ""  ),
        ( 1e+03f, "k" ),
        ( 1e+06f, "M" ),
        ( 1e+09f, "G" )
    };

    private void Awake()
    {
        impedanceChartFeed.OnImpedanceSelected += OnImpedanceSelected;
        frequencyValueText.text = activeResistanceValueText.text = capacitanceValueText.text = "-";
    }

    private void OnDestroy()
    {
        impedanceChartFeed.OnImpedanceSelected -= OnImpedanceSelected;
    }

    private void OnImpedanceSelected(ComplexDouble impedance, float frequency)
    {
        SetParameterValueText(frequencyValueText, frequency, "Hz");
        SetParameterValueText(activeResistanceValueText, impedanceComputer.ComputeActiveResistanceWithCapacitance(impedance, frequency), "Ohm");
        SetParameterValueText(capacitanceValueText, impedanceComputer.ComputeCapacitance(impedance, frequency), "F");
    }

    private void SetParameterValueText(Text target, float value, string units)
    {
        (float convertedValue, string prefix) = GetPrefixConvertedValue(value);
        target.text = $"{convertedValue:N} {prefix}{units}";
    }

    private (float, string) GetPrefixConvertedValue(float value)
    {
        float convertedValue = value;
        string foundPrefix = string.Empty;
        int lastIndex = _matchPrefixConversionMap.Count - 1;
        
        for (int i = 0; i < _matchPrefixConversionMap.Count; i++)
        {
            (float currentPrefixLimit, string currentPrefix) = _matchPrefixConversionMap[i];
            (float nextPrefixLimit, string _) = _matchPrefixConversionMap[Mathf.Clamp(i + 1, 0, lastIndex)];

            if (i == lastIndex || IsGreaterOrEqual(value, currentPrefixLimit) && IsLessOrEqual(value, nextPrefixLimit))
            {
                convertedValue = value * 1f / currentPrefixLimit;
                foundPrefix = currentPrefix;
                break;
            }
        }
        
        return (convertedValue, foundPrefix);
    }

    private bool IsLessOrEqual(float a, float b) => a < b || IsApproximateEqual(a, b);

    private bool IsGreaterOrEqual(float a, float b) => a > b || IsApproximateEqual(a, b);

    private bool IsApproximateEqual(float a, float b) => Mathf.Abs(a - b) < Mathf.Epsilon;
}
