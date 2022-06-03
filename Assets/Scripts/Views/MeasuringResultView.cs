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
    [Space]
    [SerializeField] private Button capacitanceButton;
    [SerializeField] private Button inductanceButton;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color notActiveColor = Color.gray;
    [Space]
    [SerializeField] private Text capacitanceLabelText;
    [SerializeField] private Text inductanceLabelText;
    [Space]
    [SerializeField] private Text capacitanceValueText;
    [SerializeField] private Text inductanceValueText;

    private void Awake()
    {
        capacitanceButton.onClick.AddListener(OnCapacitanceButtonClick);
        inductanceButton.onClick.AddListener(OnInductanceButtonClick);
        impedanceChartFeedView.OnImpedanceSelected += OnImpedanceSelected;
        
        frequencyValueText.text = activeResistanceValueText.text = capacitanceValueText.text = inductanceValueText.text = "-";
    }

    private void OnDestroy()
    {
        capacitanceButton.onClick.RemoveListener(OnCapacitanceButtonClick);
        inductanceButton.onClick.RemoveListener(OnInductanceButtonClick);
        impedanceChartFeedView.OnImpedanceSelected -= OnImpedanceSelected;
    }

    private void OnInductanceButtonClick()
    {
        capacitanceButton.image.color = notActiveColor;
        inductanceButton.image.color = activeColor;
        capacitanceLabelText.gameObject.SetActive(false);
        inductanceLabelText.gameObject.SetActive(true);
        capacitanceValueText.gameObject.SetActive(false);
        inductanceValueText.gameObject.SetActive(true);
    }

    private void OnCapacitanceButtonClick()
    {
        capacitanceButton.image.color = activeColor;
        inductanceButton.image.color = notActiveColor;
        capacitanceLabelText.gameObject.SetActive(true);
        inductanceLabelText.gameObject.SetActive(false);
        capacitanceValueText.gameObject.SetActive(true);
        inductanceValueText.gameObject.SetActive(false);
    }
    
    private void OnImpedanceSelected(ImpedanceMeasureData data)
    {
        SetParameterValueText(frequencyValueText, data.frequency, "Hz");
        SetParameterValueText(activeResistanceValueText, ZRLCHelper.ComputeActiveResistance(data), "Ohm");
        SetParameterValueText(capacitanceValueText, ZRLCHelper.ComputeCapacitance(data), "F");
        SetParameterValueText(inductanceValueText, ZRLCHelper.ComputeInductance(data), "H");
    }

    private void SetParameterValueText(Text target, float value, string units)
    {
        (float convertedValue, string prefix) = value.AutoConvertNormalValue();
        target.text = $"{convertedValue:N} {prefix}{units}";
    }
}
