using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Обрабатывает UI элементы, настраивающие параметры измерения.
/// </summary>
public sealed class MeasurementSetupView : MonoBehaviour
{
    public event Action<float, float> OnFrequenyRangeUpdated = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;
    
    [Header("Measurement configuration")]
    [SerializeField] private TMP_Dropdown inputReferenceChannelDropdown;
    [SerializeField] private InputFieldController referenceResistorInputField;
    [SerializeField] private TMP_Dropdown samplingRateDropdown;
    [SerializeField] private InputFieldController transientTimeInputField;

    [Header("Frequency configuration")]
    [SerializeField] private TMP_Dropdown frequencyIncrementDropdown;
    [SerializeField] private InputFieldController targetFrequencyInputField;
    [SerializeField] private InputFieldController lowCutOffFrequencyInputField;
    [SerializeField] private InputFieldController highCutOffFrequencyInputField;
    
    [Space]
    [SerializeField] private Toggle singleFrequencyToggle;
    [SerializeField] private Toggle rangeFrequencyToggle;

    private List<int> _sampleRates, _frequencyIncrements;
    private List<ReferenceChannel> _inputReferenceChannels;

    private void Awake()
    {
        InitMeasurementConfiguration();
        InitFrequencyConfiguration();
    }

    private void OnDestroy()
    {
        DestroyMeasurementConfiguration();
        DestroyFrequencyConfiguration();
    }

    private void InitMeasurementConfiguration()
    {
        referenceResistorInputField.SetValue(generalSettings.ReferenceResistance);
        transientTimeInputField.SetValue(generalSettings.TransientTimeInMs);
        
        referenceResistorInputField.OnValueEndEdit.AddListener(SetReferenceResistance);
        transientTimeInputField.OnValueEndEdit.AddListener(SetTransientTime);

        CreateDropdownFromEnumAsString<ReferenceChannel>(
            dropdown: inputReferenceChannelDropdown,
            values: ref _inputReferenceChannels,
            initValue: generalSettings.InputReferenceChannel,
            callback: SetInputReferenceChannel
        );
        
        CreateDropdownFromAsInt<SamplingRatePreset>(
            dropdown: samplingRateDropdown,
            values: ref _sampleRates,
            initValue: generalSettings.SamplingRate,
            format: "{0} Hz",
            callback: SetSamplingRate
        );
    }
    
    private void InitFrequencyConfiguration()
    {
        targetFrequencyInputField.OnValueEndEdit.AddListener(SetTargetFrequency);
        lowCutOffFrequencyInputField.OnValueEndEdit.AddListener(SetLowCutOffFrequency);
        highCutOffFrequencyInputField.OnValueEndEdit.AddListener(SetHighCutOffFrequency);
        
        targetFrequencyInputField.SetValue(generalSettings.CalibrationFrequency);
        lowCutOffFrequencyInputField.SetValue(generalSettings.LowCutOffFrequency);
        highCutOffFrequencyInputField.SetValue(generalSettings.HighCutOffFrequency);

        singleFrequencyToggle.onValueChanged.AddListener(OnSingleToggleValueChanged);
        rangeFrequencyToggle.onValueChanged.AddListener(OnRangeToggleValueChanged);
        
        OnSingleToggleValueChanged(true);
        OnRangeToggleValueChanged(false);

        CreateDropdownFromAsInt<FrequencyIncrement>(
            dropdown: frequencyIncrementDropdown,
            values: ref _frequencyIncrements,
            initValue: (int) generalSettings.FrequencyIncrement,
            format: "1/{0} octave",
            callback: SetFrequencyIncrement
        );
    }

    private void DestroyMeasurementConfiguration()
    {
        referenceResistorInputField.OnValueEndEdit.RemoveListener(SetReferenceResistance);
        transientTimeInputField.OnValueEndEdit.RemoveListener(SetTransientTime);
        inputReferenceChannelDropdown.onValueChanged.RemoveListener(SetInputReferenceChannel);
        samplingRateDropdown.onValueChanged.RemoveListener(SetSamplingRate);
    }

    private void DestroyFrequencyConfiguration()
    {
        frequencyIncrementDropdown.onValueChanged.RemoveListener(SetFrequencyIncrement);
        singleFrequencyToggle.onValueChanged.RemoveListener(OnSingleToggleValueChanged);
        rangeFrequencyToggle.onValueChanged.RemoveListener(OnRangeToggleValueChanged);
        targetFrequencyInputField.OnValueEndEdit.RemoveListener(SetTargetFrequency);
        lowCutOffFrequencyInputField.OnValueEndEdit.RemoveListener(SetLowCutOffFrequency);
        highCutOffFrequencyInputField.OnValueEndEdit.RemoveListener(SetHighCutOffFrequency);
    }

    private void SetInputReferenceChannel(int dropdownIndex)
    {
        generalSettings.InputReferenceChannel = _inputReferenceChannels[dropdownIndex];
    }

    private void SetReferenceResistance(float value)
    {
        generalSettings.ReferenceResistance = value;
    }
    
    private void SetTransientTime(float value)
    {
        generalSettings.TransientTimeInMs = value;
    }

    private void SetSamplingRate(int dropdownIndex)
    {
        generalSettings.SamplingRate = _sampleRates[dropdownIndex];
    }
    
    private void SetFrequencyIncrement(int dropdownIndex)
    {
        generalSettings.FrequencyIncrement = (FrequencyIncrement) _frequencyIncrements[dropdownIndex];
    }
    
    private void OnSingleToggleValueChanged(bool isOn)
    {
        targetFrequencyInputField.SetActive(isOn);

        if (isOn == false)
        {
            return;
        }

        SetTargetFrequency(targetFrequencyInputField.Value);
    }

    private void OnRangeToggleValueChanged(bool isOn)
    {
        lowCutOffFrequencyInputField.SetActive(isOn);
        highCutOffFrequencyInputField.SetActive(isOn);

        if (isOn == false)
        {
            return;
        }

        SetLowCutOffFrequency(lowCutOffFrequencyInputField.Value);
        SetHighCutOffFrequency(highCutOffFrequencyInputField.Value);
    }
    
    private void SetTargetFrequency(float value)
    {
        generalSettings.LowCutOffFrequency = value;
        generalSettings.HighCutOffFrequency = value;
        
        OnFrequenyRangeUpdated.Invoke(value, value);
    }
    
    private void SetLowCutOffFrequency(float value)
    {
        generalSettings.LowCutOffFrequency = value;
        OnFrequenyRangeUpdated.Invoke(value, generalSettings.HighCutOffFrequency);
    }

    private void SetHighCutOffFrequency(float value)
    {
        generalSettings.HighCutOffFrequency = value;
        OnFrequenyRangeUpdated.Invoke(generalSettings.LowCutOffFrequency, value);
    }

    private static void CreateDropdownFromEnumAsString<TEnum>(
        TMP_Dropdown dropdown,
        ref List<TEnum> values,
        TEnum initValue,
        UnityAction<int> callback
    ) where TEnum : Enum
    {
        values = Enum.GetValues(typeof(TEnum)).OfType<TEnum>().ToList();
        
        dropdown.options = values
            .Select(value => new TMP_Dropdown.OptionData(text: value.ToString()))
            .ToList();
        
        dropdown.RefreshShownValue();
        dropdown.value = values.IndexOf(initValue);

        dropdown.onValueChanged.AddListener(callback);
    }
    
    private static void CreateDropdownFromAsInt<TEnum>(
        TMP_Dropdown dropdown,
        ref List<int> values,
        int initValue,
        string format,
        UnityAction<int> callback
    ) where TEnum : Enum
    {
        values = Enum.GetValues(typeof(TEnum)).Cast<int>().ToList();
        
        dropdown.options = values
            .Select(value => new TMP_Dropdown.OptionData(text: string.Format(format, value)))
            .ToList();
        
        dropdown.RefreshShownValue();
        dropdown.value = values.IndexOf(initValue);

        dropdown.onValueChanged.AddListener(callback);
    }
}
