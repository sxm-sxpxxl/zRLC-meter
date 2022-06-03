using UnityEngine;
using UnityEngine.UI;

// todo: refactoring
/// <summary>
/// Обрабатывает нажатия на кнопки калибровки.
/// </summary>
[DisallowMultipleComponent]
public sealed class CalibrationView : MonoBehaviour
{
    [SerializeField] private ChannelsCalibrator channelsCalibrator;
    
    [Space]
    [SerializeField] private Button openCalibrationButton;
    [SerializeField] private Button shortCalibrationButton;

    private void Awake()
    {
        openCalibrationButton.onClick.AddListener(OnOpenCalibrationButtonClick);
        shortCalibrationButton.onClick.AddListener(OnShortCalibrationButtonClick);
        
        channelsCalibrator.OnCalibrationErrorOccurred += OnCalibrationErrorOccurred;
        channelsCalibrator.OnCalibrationFinished += OnCalibrationFinished;
    }

    private void OnDestroy()
    {
        openCalibrationButton.onClick.RemoveListener(OnOpenCalibrationButtonClick);
        channelsCalibrator.OnCalibrationErrorOccurred -= OnCalibrationErrorOccurred;
        channelsCalibrator.OnCalibrationFinished -= OnCalibrationFinished;
    }

    private void Start()
    {
        openCalibrationButton.interactable = true;
        shortCalibrationButton.interactable = true;
    }

    private void OnOpenCalibrationButtonClick()
    {
        openCalibrationButton.interactable = false;
        shortCalibrationButton.interactable = false;
        
        channelsCalibrator.OpenCalibrate();
    }

    private void OnShortCalibrationButtonClick()
    {
        openCalibrationButton.interactable = false;
        shortCalibrationButton.interactable = false;
        
        channelsCalibrator.ShortCalibrate();
    }

    private void OnCalibrationFinished()
    {
        openCalibrationButton.interactable = true;
        shortCalibrationButton.interactable = true;
    }

    private void OnCalibrationErrorOccurred(string message)
    {
        openCalibrationButton.interactable = true;
        shortCalibrationButton.interactable = true;
    }
}
