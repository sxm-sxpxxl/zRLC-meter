using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Обрабатывает UI элементы, настраивающие параметры измерения.
/// </summary>
public sealed class MeasuringSettingsView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;
    
    [Header("Related views")]
    [SerializeField] private InputField lowCutOffFrequencyInputField;
    [SerializeField] private InputField highCutOffFrequencyInputField;
    [SerializeField] private InputField equivalenceResistanceInputField;
    [SerializeField] private Dropdown sampleRateDropdown;

    private List<int> _sampleRates;

    private void Awake()
    {
        SetValueToInputField(lowCutOffFrequencyInputField, generalSettings.LowCutOffFrequency);
        SetValueToInputField(highCutOffFrequencyInputField, generalSettings.HighCutOffFrequency);
        SetValueToInputField(equivalenceResistanceInputField, generalSettings.EquivalenceResistance);

        lowCutOffFrequencyInputField.onEndEdit.AddListener(SetLowCutOffFrequency);
        highCutOffFrequencyInputField.onEndEdit.AddListener(SetHighCutOffFrequency);
        equivalenceResistanceInputField.onEndEdit.AddListener(SetEquivalenceResistance);

        _sampleRates = Enum.GetValues(typeof(SamplingRatePreset)).Cast<int>().ToList();
        
        sampleRateDropdown.options = _sampleRates
            .Select(value => new Dropdown.OptionData(text: $"{value} Hz"))
            .ToList();
        
        sampleRateDropdown.RefreshShownValue();
        sampleRateDropdown.value = _sampleRates.IndexOf(generalSettings.SamplingRate);

        sampleRateDropdown.onValueChanged.AddListener(SetSampleRate);
    }

    private void OnDestroy()
    {
        lowCutOffFrequencyInputField.onEndEdit.RemoveListener(SetLowCutOffFrequency);
        highCutOffFrequencyInputField.onEndEdit.RemoveListener(SetHighCutOffFrequency);
        equivalenceResistanceInputField.onEndEdit.RemoveListener(SetEquivalenceResistance);
        sampleRateDropdown.onValueChanged.RemoveListener(SetSampleRate);
    }

    private void SetLowCutOffFrequency(string value)
    {
        generalSettings.LowCutOffFrequency = int.Parse(value);
    }

    private void SetHighCutOffFrequency(string value)
    {
        generalSettings.HighCutOffFrequency = int.Parse(value);
    }
    
    private void SetEquivalenceResistance(string value)
    {
        generalSettings.EquivalenceResistance = int.Parse(value);
    }

    private void SetSampleRate(int dropdownIndex)
    {
        generalSettings.SamplingRate = _sampleRates[dropdownIndex];
    }

    private void SetValueToInputField(InputField target, float value)
    {
        target.text = Mathf.RoundToInt(value).ToString();
    }
}
