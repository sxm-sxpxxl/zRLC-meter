using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MeasuringResultView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ImpedanceChartFeedView impedanceChartFeedView;
    
    [Header("Related views")]
    [SerializeField] private Text frequencyValueText;
    [SerializeField] private Text activeResistanceValueText;
    [SerializeField] private Text capacitanceValueText;

    private void Awake()
    {
        impedanceChartFeedView.OnImpedanceSelected += OnImpedanceSelected;
        frequencyValueText.text = activeResistanceValueText.text = capacitanceValueText.text = "-";
    }

    private void OnDestroy()
    {
        impedanceChartFeedView.OnImpedanceSelected -= OnImpedanceSelected;
    }

    private void OnImpedanceSelected(ImpedanceMeasureData data)
    {
        SetParameterValueText(frequencyValueText, data.frequency, "Hz");
        SetParameterValueText(
            activeResistanceValueText,
            ZRLCHelper.ComputeActiveResistanceWithCapacitance(data.magnitude, data.phaseInDeg, data.frequency),
            "Ohm"
        );
        SetParameterValueText(
            capacitanceValueText,
            ZRLCHelper.ComputeCapacitance(data.magnitude, data.phaseInDeg, data.frequency),
            "F"
        );
    }

    private void SetParameterValueText(Text target, float value, string units)
    {
        (float convertedValue, string prefix) = value.PrefixConvertedValue();
        target.text = $"{convertedValue:N} {prefix}{units}";
    }
}
