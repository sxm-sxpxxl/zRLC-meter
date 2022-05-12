using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Обрабатывает нажатия на кнопки запуска и остановки процесса измерения.
/// </summary>
[DisallowMultipleComponent]
public sealed class MeasuringProcessView : MonoBehaviour
{
    [SerializeField] private ImpedanceMeasurer impedanceMeasurer;
    
    [Space]
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;

    private void Awake()
    {
        startButton.onClick.AddListener(OnClickStartButton);
        stopButton.onClick.AddListener(OnClickStopButton);
        impedanceMeasurer.OnImpedanceMeasuringFinished += SetStopButtonMode;
        
        SetStopButtonMode();
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(OnClickStartButton);
        stopButton.onClick.RemoveListener(OnClickStopButton);
        impedanceMeasurer.OnImpedanceMeasuringFinished -= SetStopButtonMode;
    }

    private void OnClickStartButton()
    {
        SetStartButtonMode();
        impedanceMeasurer.StartMeasuring();
    }
    
    private void OnClickStopButton()
    {
        SetStopButtonMode();
        impedanceMeasurer.StopMeasuring();
    }

    private void SetStartButtonMode() => SetButtonMode(false);
    
    private void SetStopButtonMode() => SetButtonMode(true);
    
    private void SetButtonMode(bool isStartButtonInteractable)
    {
        startButton.interactable = isStartButtonInteractable;
        stopButton.interactable = !isStartButtonInteractable;
    }
}
