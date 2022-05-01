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

    private void OnImpedanceSelected(float impedanceMagnitude, float frequency)
    {
        SetParameterValueText(frequencyValueText, frequency, "Hz");
        // TODO: implement it
        // SetParameterValueText(activeResistanceValueText, impedanceComputer.ComputeActiveResistanceWithCapacitance(impedanceMagnitude, frequency), "Ohm");
        // SetParameterValueText(capacitanceValueText, impedanceComputer.ComputeCapacitance(impedanceMagnitude, frequency), "F");
    }

    private void SetParameterValueText(Text target, float value, string units)
    {
        (float convertedValue, string prefix) = value.PrefixConvertedValue();
        target.text = $"{convertedValue:N} {prefix}{units}";
    }
}
