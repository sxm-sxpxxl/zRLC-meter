using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Отслеживает нажатия на точки графиков амплитуды и фазы импеданса и для соответствующего импеданса рассчитывает и отображает RLC параметры.
/// </summary>
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
        SetParameterValueText(activeResistanceValueText, ZRLCHelper.ComputeActiveResistance(data), "Ohm");
        SetParameterValueText(capacitanceValueText, ZRLCHelper.ComputeCapacitance(data), "F");
    }

    private void SetParameterValueText(Text target, float value, string units)
    {
        (float convertedValue, string prefix) = value.AutoConvertNormalValue();
        target.text = $"{convertedValue:N} {prefix}{units}";
    }
}
