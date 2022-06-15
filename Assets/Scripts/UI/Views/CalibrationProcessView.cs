using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Обрабатывает нажатия на кнопки калибровки.
/// </summary>
[DisallowMultipleComponent]
public sealed class CalibrationProcessView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ChannelsCalibrator channelsCalibrator;
    
    [Header("Calibration Process")]
    [SerializeField] private Button openCalibrationButton;
    [SerializeField] private Button shortCalibrationButton;

    [Space]
    [SerializeField] private InputFieldController inputImpedanceInputField;
    [SerializeField] private InputFieldController groundImpedanceInputField;
    
    private void Awake()
    {
        openCalibrationButton.onClick.AddListener(OnOpenCalibrationButtonClick);
        shortCalibrationButton.onClick.AddListener(OnShortCalibrationButtonClick);
        
        channelsCalibrator.OnCalibrationErrorOccurred += OnCalibrationErrorOccurred;
        channelsCalibrator.OnOpenCalibrationFinished += OnOpenCalibrationFinished;
        channelsCalibrator.OnShortCalibrationFinished += OnShortCalibrationFinished;
    }

    private void OnDestroy()
    {
        openCalibrationButton.onClick.RemoveListener(OnOpenCalibrationButtonClick);
        shortCalibrationButton.onClick.AddListener(OnShortCalibrationButtonClick);
        
        channelsCalibrator.OnCalibrationErrorOccurred -= OnCalibrationErrorOccurred;
        channelsCalibrator.OnOpenCalibrationFinished -= OnOpenCalibrationFinished;
        channelsCalibrator.OnShortCalibrationFinished -= OnShortCalibrationFinished;
    }

    private void Start()
    {
        DeactivateCalibrationMode();
    }

    private void OnOpenCalibrationButtonClick()
    {
        ActivateCalibrationMode();
        channelsCalibrator.OpenCalibrate();
    }

    private void OnShortCalibrationButtonClick()
    {
        ActivateCalibrationMode();
        channelsCalibrator.ShortCalibrate();
    }

    private void OnOpenCalibrationFinished(ComplexFloat inputImpedance)
    {
        DeactivateCalibrationMode();
        inputImpedanceInputField.SetValue(inputImpedance.Magnitude);
    }

    private void OnShortCalibrationFinished(ComplexFloat groundImpedance)
    {
        DeactivateCalibrationMode();
        groundImpedanceInputField.SetValue(groundImpedance.Magnitude);
    }

    private void OnCalibrationErrorOccurred(string message)
    {
        DeactivateCalibrationMode();
    }

    private void ActivateCalibrationMode()
    {
        openCalibrationButton.interactable = false;
        shortCalibrationButton.interactable = false;
    }

    private void DeactivateCalibrationMode()
    {
        openCalibrationButton.interactable = true;
        shortCalibrationButton.interactable = true;
    }
}
