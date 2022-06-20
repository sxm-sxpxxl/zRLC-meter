using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Обрабатывает нажатия на кнопки калибровки.
/// </summary>
[DisallowMultipleComponent]
public sealed class CalibrationProcessView : MonoBehaviour
{
    public event Action<ComplexFloat> OnGainCorrectionCalibrated = delegate { };
    public event Action<ComplexFloat> OnInputImpedanceCalibrated = delegate { };
    public event Action<ComplexFloat> OnGroundImpedanceCalibrated = delegate { };

    [Header("Dependencies")]
    [SerializeField] private ChannelsCalibrator channelsCalibrator;

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
        
        channelsCalibrator.OnCalibrationErrorOccurred += OnCalibrationErrorOccurred;
        channelsCalibrator.OnGainCalibrationFinished += OnGainCalibrationFinished;
        channelsCalibrator.OnOpenCalibrationFinished += OnOpenCalibrationFinished;
        channelsCalibrator.OnShortCalibrationFinished += OnShortCalibrationFinished;
    }

    private void OnDestroy()
    {
        gainCalibrationButton.onClick.RemoveListener(OnGainCalibrationButtonClick);
        openCalibrationButton.onClick.RemoveListener(OnOpenCalibrationButtonClick);
        shortCalibrationButton.onClick.AddListener(OnShortCalibrationButtonClick);
        
        channelsCalibrator.OnCalibrationErrorOccurred -= OnCalibrationErrorOccurred;
        channelsCalibrator.OnGainCalibrationFinished -= OnGainCalibrationFinished;
        channelsCalibrator.OnOpenCalibrationFinished -= OnOpenCalibrationFinished;
        channelsCalibrator.OnShortCalibrationFinished -= OnShortCalibrationFinished;
    }

    private void OnGainCalibrationButtonClick()
    {
        ActivateCalibrationMode();
        channelsCalibrator.GainCalibrate();
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

    private void OnGainCalibrationFinished(ComplexFloat gainCorrectionRatio)
    {
        DeactivateCalibrationMode();
        gainCorrectionInputField.SetValue(gainCorrectionRatio.Magnitude);
        OnGainCorrectionCalibrated.Invoke(gainCorrectionRatio);
    }
    
    private void OnOpenCalibrationFinished(ComplexFloat inputImpedance)
    {
        DeactivateCalibrationMode();
        inputImpedanceInputField.SetValue(inputImpedance.Magnitude);
        OnInputImpedanceCalibrated.Invoke(inputImpedance);
    }

    private void OnShortCalibrationFinished(ComplexFloat groundImpedance)
    {
        DeactivateCalibrationMode();
        groundImpedanceInputField.SetValue(groundImpedance.Magnitude);
        OnGroundImpedanceCalibrated.Invoke(groundImpedance);
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
