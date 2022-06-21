using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Отслеживает нажатия на точки графиков амплитуды и фазы импеданса и для соответствующего импеданса рассчитывает и отображает RLC параметры.
/// </summary>
[DisallowMultipleComponent]
public sealed class MeasurementProcessView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private MeasurementProcessController measurementProcessController;
    [SerializeField] private ImpedanceChartFeedView impedanceChartFeedView;
    [SerializeField] private GraphScreenshotSaver graphScreenshotSaver;

    [Header("Measurement Process")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [Space]
    [SerializeField] private Toggle capacitanceToggle;
    [SerializeField] private Toggle inductanceToggle;
    [Space]
    [SerializeField] private InputFieldController frequencyInputField;
    [SerializeField] private InputFieldController activeResistanceInputField;
    [SerializeField] private InputFieldController capacitanceInputField;
    [SerializeField] private InputFieldController inductanceInputField;
    [Space]
    [SerializeField] private Button clearResultsButton;
    [SerializeField] private Button saveGraphButton;

    private void Awake()
    {
        measurementProcessController.OnImpedanceMeasured += OnImpedanceSelected;
        impedanceChartFeedView.OnImpedanceSelected += OnImpedanceSelected;

        measurementProcessController.OnImpedanceMeasuringFinished += SetStopButtonMode;
        startButton.onClick.AddListener(OnStartButtonClick);
        stopButton.onClick.AddListener(OnStopButtonClick);
        
        SetStopButtonMode();
        
        capacitanceToggle.onValueChanged.AddListener(OnCapacitanceToggleValueChanged);
        inductanceToggle.onValueChanged.AddListener(OnInductanceToggleValueChanged);
        
        clearResultsButton.onClick.AddListener(OnClearResultsButtonClick);
        saveGraphButton.onClick.AddListener(OnSaveGraphButtonClick);

        ResetResults();
    }

    private void OnDestroy()
    {
        measurementProcessController.OnImpedanceMeasured -= OnImpedanceSelected;
        impedanceChartFeedView.OnImpedanceSelected -= OnImpedanceSelected;
        
        measurementProcessController.OnImpedanceMeasuringFinished -= SetStopButtonMode;
        startButton.onClick.RemoveListener(OnStartButtonClick);
        stopButton.onClick.RemoveListener(OnStopButtonClick);
        
        capacitanceToggle.onValueChanged.RemoveListener(OnCapacitanceToggleValueChanged);
        inductanceToggle.onValueChanged.RemoveListener(OnInductanceToggleValueChanged);
        
        clearResultsButton.onClick.RemoveListener(OnClearResultsButtonClick);
        saveGraphButton.onClick.RemoveListener(OnSaveGraphButtonClick);
    }

    private void OnCapacitanceToggleValueChanged(bool isOn)
    {
        capacitanceInputField.SetActive(isOn);
    }
    
    private void OnInductanceToggleValueChanged(bool isOn)
    {
        inductanceInputField.SetActive(isOn);
    }

    private void OnImpedanceSelected(ImpedanceMeasureData data)
    {
        frequencyInputField.SetValue(data.frequency);
        activeResistanceInputField.SetValue(ZRLCHelper.ComputeActiveResistance(data));
        capacitanceInputField.SetValue(ZRLCHelper.ComputeCapacitance(data));
        inductanceInputField.SetValue(ZRLCHelper.ComputeInductance(data));
    }
    
    private void OnStartButtonClick()
    {
        SetStartButtonMode();
        measurementProcessController.StartMeasuring();
    }
    
    private void OnStopButtonClick()
    {
        SetStopButtonMode();
        measurementProcessController.StopMeasuring();
    }

    private void OnClearResultsButtonClick()
    {
        ResetResults();
        impedanceChartFeedView.ClearChartData();
    }

    private void OnSaveGraphButtonClick()
    {
        graphScreenshotSaver.TakeScreenshot();
    }
    
    private void SetStartButtonMode() => SetButtonMode(false);
    
    private void SetStopButtonMode() => SetButtonMode(true);
    
    private void SetButtonMode(bool isStartButtonInteractable)
    {
        startButton.gameObject.SetActive(isStartButtonInteractable);
        stopButton.gameObject.SetActive(!isStartButtonInteractable);
    }
    
    private void ResetResults()
    {
        frequencyInputField.ResetValue();
        activeResistanceInputField.ResetValue();
        capacitanceInputField.ResetValue();
        inductanceInputField.ResetValue();
    }
}
