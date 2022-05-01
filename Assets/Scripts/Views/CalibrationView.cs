using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CalibrationView : MonoBehaviour
{
    [SerializeField] private ChannelsCalibrator channelsCalibrator;
    
    [Space]
    [SerializeField] private Button calibrationButton;
    [SerializeField] private Text calibrationValueText;

    private void Awake()
    {
        calibrationButton.onClick.AddListener(OnCalibrationButtonClick);
        channelsCalibrator.OnCalibrationFinished += ShowCalibrationRatioLevel;
    }

    private void OnDestroy()
    {
        calibrationButton.onClick.RemoveListener(OnCalibrationButtonClick);
        channelsCalibrator.OnCalibrationFinished -= ShowCalibrationRatioLevel;
    }

    private void Start()
    {
        calibrationValueText.text = "-";
        calibrationButton.interactable = true;
    }

    private void OnCalibrationButtonClick()
    {
        calibrationButton.interactable = false;
        channelsCalibrator.Calibrate();
    }

    private void ShowCalibrationRatioLevel(float calibrationRatioLevel)
    {
        calibrationValueText.text = $"{calibrationRatioLevel:N} dBFS";
        calibrationButton.interactable = true;
    }
}
