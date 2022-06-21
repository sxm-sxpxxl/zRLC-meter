using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MeasurementModeView : MonoBehaviour
{
    [SerializeField] private MeasurementProcessController measurementProcessController;
    [SerializeField] private CalibrationProcessController calibrationProcessController;
    [SerializeField] private TestGenerationProcessController testGenerationProcessController;
    [SerializeField] private TabGroup tabGroup;
    
    [Header("Progress bar settings")]
    [SerializeField] private Image measurementProcessProgressBar;
    [SerializeField] private Image gainCalibrationProcessProgressBar;
    [SerializeField] private Image openCalibrationProcessProgressBar;
    [SerializeField] private Image shortCalibrationProcessProgressBar;
    [SerializeField] private Image testGenerationProcessProgressBar;
    
    [Header("Dash border settings")]
    [SerializeField] private GameObject container;
    [SerializeField, Range(0f, 1f)] private float foldoutDurationInSec = 0.1f;
    [SerializeField] private Material horizontalDashMaterial;
    [SerializeField] private Material verticalDashMaterial;
    [SerializeField, Range(0f, 10f)] private float speed = 1f;

    private Coroutine _dashBorderAnimationCoroutine, _foldoutDashBorderCoroutine;
    
    private void Awake()
    {
        DeactivateMeasurementMode();
        
        measurementProcessController.OnImpedanceMeasuringStarted += ActivateMeasurementMode;
        calibrationProcessController.OnCalibrationStarted += ActivateMeasurementMode;
        testGenerationProcessController.OnTestGenerationStarted += ActivateMeasurementMode;

        measurementProcessController.OnImpedanceMeasuringProgressUpdated += OnImpedanceMeasuringProgressUpdated;
        calibrationProcessController.OnCalibrationProgressUpdated += OnCalibrationProgressUpdated;
        testGenerationProcessController.OnTestGenerationProgressUpdated += OnTestGenerationProgressUpdated;
        
        measurementProcessController.OnImpedanceMeasuringFinished += DeactivateMeasurementMode;
        calibrationProcessController.OnCalibrationFinished += DeactivateMeasurementMode;
        testGenerationProcessController.OnTestGenerationFinished += DeactivateMeasurementMode;
    }

    private void OnDestroy()
    {
        measurementProcessController.OnImpedanceMeasuringStarted -= ActivateMeasurementMode;
        calibrationProcessController.OnCalibrationStarted -= ActivateMeasurementMode;
        testGenerationProcessController.OnTestGenerationStarted -= ActivateMeasurementMode;
        
        measurementProcessController.OnImpedanceMeasuringProgressUpdated -= OnImpedanceMeasuringProgressUpdated;
        calibrationProcessController.OnCalibrationProgressUpdated -= OnCalibrationProgressUpdated;
        testGenerationProcessController.OnTestGenerationProgressUpdated -= OnTestGenerationProgressUpdated;
        
        measurementProcessController.OnImpedanceMeasuringFinished -= DeactivateMeasurementMode;
        calibrationProcessController.OnCalibrationFinished -= DeactivateMeasurementMode;
        testGenerationProcessController.OnTestGenerationFinished -= DeactivateMeasurementMode;
    }

    private void ActivateMeasurementMode()
    {
        if (_dashBorderAnimationCoroutine != null)
        {
            StopCoroutine(_dashBorderAnimationCoroutine);
        }
        
        _dashBorderAnimationCoroutine = StartCoroutine(DashBorderAnimationCoroutine());
        tabGroup.DisableUnselectedTabs();
    }

    private void OnImpedanceMeasuringProgressUpdated(float progress) =>
        UpdateProgressBar(measurementProcessProgressBar, progress);

    private void OnCalibrationProgressUpdated(CalibrationProcessController.CalibrationType type, float progress)
    {
        Image targetProgressBar = type switch
        {
            CalibrationProcessController.CalibrationType.Gain => gainCalibrationProcessProgressBar,
            CalibrationProcessController.CalibrationType.Open => openCalibrationProcessProgressBar,
            CalibrationProcessController.CalibrationType.Short => shortCalibrationProcessProgressBar,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        UpdateProgressBar(targetProgressBar, progress);
    }

    private void OnTestGenerationProgressUpdated(float progress) =>
        UpdateProgressBar(testGenerationProcessProgressBar, progress);

    private void UpdateProgressBar(Image progressBar, float progress)
    {
        progress = Mathf.Clamp01(progress);
        progressBar.fillAmount = progress;
    }

    private void DeactivateMeasurementMode(float _1, float _2, float _3) => DeactivateMeasurementMode();
    
    private void DeactivateMeasurementMode(CalibrationProcessController.CalibrationType _1, ComplexFloat _2) 
        => DeactivateMeasurementMode();
    
    private void DeactivateMeasurementMode()
    {
        if (_dashBorderAnimationCoroutine != null)
        {
            StopCoroutine(_dashBorderAnimationCoroutine);            
        }
        
        StartCoroutine(FoldoutDashBorderCoroutine(needToOpen: false));

        UpdateProgressBar(measurementProcessProgressBar, 0f);
        UpdateProgressBar(gainCalibrationProcessProgressBar, 0f);
        UpdateProgressBar(openCalibrationProcessProgressBar, 0f);
        UpdateProgressBar(shortCalibrationProcessProgressBar, 0f);
        UpdateProgressBar(testGenerationProcessProgressBar, 0f);
        
        tabGroup.EnableUnselectedTabs();
    }

    private IEnumerator DashBorderAnimationCoroutine()
    {
        yield return StartCoroutine(FoldoutDashBorderCoroutine(needToOpen: true));

        while (true)
        {
            float nextOffset = Mathf.Repeat(speed * Time.time, 1f);
            
            horizontalDashMaterial.mainTextureOffset = new Vector2(nextOffset, 0f);
            verticalDashMaterial.mainTextureOffset = new Vector2(0f, nextOffset);

            yield return null;
        }
    }

    private IEnumerator FoldoutDashBorderCoroutine(bool needToOpen)
    {
        if (needToOpen)
        {
            container.SetActive(true);
        }
        
        float elapsedTime = 0f;
        float startValue = needToOpen ? 0f : 1f;
        float endValue = needToOpen ? 1f : 0f;

        while (elapsedTime < foldoutDurationInSec)
        {
            elapsedTime += Time.deltaTime;
            float nextAlpha = Mathf.Lerp(startValue, endValue, elapsedTime / foldoutDurationInSec);
            
            SetAlphaTo(verticalDashMaterial, nextAlpha);
            SetAlphaTo(horizontalDashMaterial, nextAlpha);
            
            yield return null;
        }


        if (needToOpen == false)
        {
            container.SetActive(false);   
        }
    }

    private void SetAlphaTo(Material material, float alpha)
    {
        Color color = material.color;
        color.a = alpha;
        material.color = color;
    }
}
