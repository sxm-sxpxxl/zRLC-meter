using UnityEngine;
using UnityEngine.UI;

public sealed class CalibrationProcessPageValidator : PageValidator
{
    [Header("Dependencies")]
    [SerializeField] private CalibrationProcessView calibrationProcessView;
    [Space]
    [SerializeField] private Selectable gainCalibrationButton;
    [SerializeField] private Selectable openCalibrationButton;
    [SerializeField] private Selectable shortCalibrationButton;

    [Header("Validation info boxes")]
    [SerializeField] private GameObject noneGainCorrectionInfoBox;
    [SerializeField] private GameObject noneInputImpedanceInfoBox;
    [SerializeField] private GameObject noneGroundImpedanceInfoBox;
    [Space]
    [SerializeField] private GameObject wrongGainCorrectionInfoBox;
    [SerializeField] private GameObject wrongInputImpedanceInfoBox;
    [SerializeField] private GameObject wrongGroundImpedanceInfoBox;

    private bool _isGainCorrectionValid, _isInputImpedanceValid, _isGroundImpedanceValid;

    protected override void Init()
    {
        base.Init();
        
        noneGainCorrectionInfoBox.SetActive(true);
        noneInputImpedanceInfoBox.SetActive(true);
        noneGroundImpedanceInfoBox.SetActive(true);

        wrongGainCorrectionInfoBox.SetActive(false);
        wrongInputImpedanceInfoBox.SetActive(false);
        wrongGroundImpedanceInfoBox.SetActive(false);

        _isGainCorrectionValid = _isInputImpedanceValid = _isGroundImpedanceValid = false;
        UpdateValidationState();
        
        calibrationProcessView.OnGainCorrectionCalibrated += OnGainCorrectionCalibrated;
        calibrationProcessView.OnInputImpedanceCalibrated += OnInputImpedanceCalibrated;
        calibrationProcessView.OnGroundImpedanceCalibrated += OnGroundImpedanceCalibrated;
    }

    protected override void Deinit()
    {
        base.Deinit();
        calibrationProcessView.OnGainCorrectionCalibrated -= OnGainCorrectionCalibrated;
        calibrationProcessView.OnInputImpedanceCalibrated -= OnInputImpedanceCalibrated;
        calibrationProcessView.OnGroundImpedanceCalibrated -= OnGroundImpedanceCalibrated;
    }

    protected override void OnDependencyStateChanged(bool isErrorsExist, bool isWarningsExist)
    {
        gainCalibrationButton.interactable = openCalibrationButton.interactable = shortCalibrationButton.interactable = !isErrorsExist;
    }

    private void OnGainCorrectionCalibrated(ComplexFloat gainCorrectionRatio)
    {
        const float maxGainCorrectionRatio = 10f, minGainCorrectionRatio = 0.1f;
        float magnitude = gainCorrectionRatio.Magnitude;

        _isGainCorrectionValid = magnitude > minGainCorrectionRatio && magnitude < maxGainCorrectionRatio;
        noneGainCorrectionInfoBox.SetActive(false);
        wrongGainCorrectionInfoBox.SetActive(_isGainCorrectionValid == false);
        
        UpdateValidationState();
    }

    private void OnInputImpedanceCalibrated(ComplexFloat inputImpedance)
    {
        const float minInputImpedanceMagnitude = 1f;
        _isInputImpedanceValid = inputImpedance.Magnitude > minInputImpedanceMagnitude;
        
        noneInputImpedanceInfoBox.SetActive(false);
        wrongInputImpedanceInfoBox.SetActive(_isInputImpedanceValid == false);
        
        UpdateValidationState();
    }

    private void OnGroundImpedanceCalibrated(ComplexFloat groundImpedance)
    {
        const float maxGroundImpedanceMagnitude = 1f;
        _isGroundImpedanceValid = groundImpedance.Magnitude < maxGroundImpedanceMagnitude;
        
        noneGroundImpedanceInfoBox.SetActive(false);
        wrongGroundImpedanceInfoBox.SetActive(_isGroundImpedanceValid == false);
        
        UpdateValidationState();
    }

    private void UpdateValidationState()
    {
        IsValid = _isGainCorrectionValid && _isInputImpedanceValid && _isGroundImpedanceValid;
    }
}
