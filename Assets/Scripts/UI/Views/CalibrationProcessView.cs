using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Обрабатывает нажатия на кнопки калибровки.
/// </summary>
[DisallowMultipleComponent]
public sealed class CalibrationProcessView : MonoBehaviour
{
    public event Action<CalibrationProcessController.CalibrationType, ComplexFloat> OnCalibrationValueChanged = delegate { };

    [Header("Dependencies")]
    [SerializeField] private CalibrationProcessController calibrationProcessController;

    [Header("Calibration Process")]
    [SerializeField] private Button gainCalibrationButton;
    [SerializeField] private Button openCalibrationButton;
    [SerializeField] private Button shortCalibrationButton;

    [Space]
    [SerializeField] private InputFieldController gainCorrectionInputField;
    [SerializeField] private InputFieldController inputImpedanceInputField;
    [SerializeField] private InputFieldController groundImpedanceInputField;
    
    private void Awake()
    {
        DeactivateCalibrationMode();

        gainCalibrationButton.onClick.AddListener(OnGainCalibrationButtonClick);
        openCalibrationButton.onClick.AddListener(OnOpenCalibrationButtonClick);
        shortCalibrationButton.onClick.AddListener(OnShortCalibrationButtonClick);
        
        calibrationProcessController.OnCalibrationErrorOccurred += OnCalibrationErrorOccurred;
        calibrationProcessController.OnCalibrationFinished += OnCalibrationFinished;
    }

    private void OnDestroy()
    {
        gainCalibrationButton.onClick.RemoveListener(OnGainCalibrationButtonClick);
        openCalibrationButton.onClick.RemoveListener(OnOpenCalibrationButtonClick);
        shortCalibrationButton.onClick.AddListener(OnShortCalibrationButtonClick);
        
        calibrationProcessController.OnCalibrationErrorOccurred -= OnCalibrationErrorOccurred;
        calibrationProcessController.OnCalibrationFinished -= OnCalibrationFinished;
    }

    private void OnGainCalibrationButtonClick()
    {
        ActivateCalibrationMode();
        calibrationProcessController.GainCalibrate();
    }

    private void OnOpenCalibrationButtonClick()
    {
        ActivateCalibrationMode();
        calibrationProcessController.OpenCalibrate();
    }

    private void OnShortCalibrationButtonClick()
    {
        ActivateCalibrationMode();
        calibrationProcessController.ShortCalibrate();
    }

    private void OnCalibrationFinished(CalibrationProcessController.CalibrationType type, ComplexFloat result)
    {
        DeactivateCalibrationMode();

        InputFieldController targetInputField = type switch
        {
            CalibrationProcessController.CalibrationType.Gain => gainCorrectionInputField,
            CalibrationProcessController.CalibrationType.Open => inputImpedanceInputField,
            CalibrationProcessController.CalibrationType.Short => groundImpedanceInputField,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        targetInputField.SetValue(result.Magnitude);
        OnCalibrationValueChanged.Invoke(type, result);
    }

    private void OnCalibrationErrorOccurred(string message)
    {
        DeactivateCalibrationMode();
    }

    private void ActivateCalibrationMode()
    {
        gainCalibrationButton.interactable = false;
        openCalibrationButton.interactable = false;
        shortCalibrationButton.interactable = false;
    }

    private void DeactivateCalibrationMode()
    {
        gainCalibrationButton.interactable = true;
        openCalibrationButton.interactable = true;
        shortCalibrationButton.interactable = true;
    }
}
