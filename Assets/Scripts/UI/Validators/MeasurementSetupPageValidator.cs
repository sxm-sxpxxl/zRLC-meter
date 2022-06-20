using System;
using UnityEngine;

public sealed class MeasurementSetupPageValidator : PageValidator
{
    [Header("Dependencies")]
    [SerializeField] private MeasurementSetupView measurementSetupView;

    [Header("Validation info boxes")]
    [SerializeField] private GameObject wrongFrequencyRangeInfoBox;

    protected override void Init()
    {
        base.Init();
        measurementSetupView.OnFrequenyRangeUpdated += OnFrequencyRangeUpdated;
    }

    protected override void Deinit()
    {
        base.Deinit();
        measurementSetupView.OnFrequenyRangeUpdated -= OnFrequencyRangeUpdated;
    }

    private void OnFrequencyRangeUpdated(float lowCutOffFrequency, float highCutOffFrequency)
    {
        IsValid = lowCutOffFrequency < highCutOffFrequency || Math.Abs(lowCutOffFrequency - highCutOffFrequency) < float.Epsilon;
        wrongFrequencyRangeInfoBox.SetActive(IsValid == false);
    }
}
