using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using SoundIO.SimpleDriver;
using DeviceType = SoundIO.SimpleDriver.DeviceType;

/// <summary>
/// Обрабатывает UI элементы, настраивающие аудио-устройства ввода-вывода.
/// </summary>
[DisallowMultipleComponent]
public sealed class SoundcardSetupView : MonoBehaviour
{
    public event Action<int, int> OnInputDeviceSelected = delegate { };
    public event Action<int> OnOutputDeviceSelected = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;
    
    [Header("Soundcard Setup")]
    [SerializeField] private TMP_Dropdown outputDeviceDropdown;
    [SerializeField] private TMP_Dropdown inputDeviceDropdown;
    [SerializeField] private TMP_Text inputChannelCountText;

    private List<int> _sharedInputDeviceIndexMap, _sharedOutputDeviceIndexMap;

    private void Start()
    {
        CreateDeviceOptionsFor(outputDeviceDropdown, DeviceType.Output, DeviceDriver.OutputDeviceCount);
        outputDeviceDropdown.value = _sharedOutputDeviceIndexMap.FindIndex(0, i => i == DeviceDriver.DefaultOutputDeviceIndex);
        outputDeviceDropdown.onValueChanged.AddListener(SetOutputDevice);
        SetOutputDevice(outputDeviceDropdown.value);

        CreateDeviceOptionsFor(inputDeviceDropdown, DeviceType.Input, DeviceDriver.InputDeviceCount);
        inputDeviceDropdown.value = _sharedOutputDeviceIndexMap.FindIndex(0, i => i == DeviceDriver.DefaultInputDeviceIndex);
        inputDeviceDropdown.onValueChanged.AddListener(SetInputDevice);
        SetInputDevice(inputDeviceDropdown.value);
    }
    
    private void OnDestroy()
    {
        outputDeviceDropdown.onValueChanged.RemoveListener(SetOutputDevice);
        inputDeviceDropdown.onValueChanged.RemoveListener(SetInputDevice);
    }
    
    private void SetOutputDevice(int dropdownIndex)
    {
        generalSettings.OutputDeviceIndex = _sharedOutputDeviceIndexMap[dropdownIndex];
        OnOutputDeviceSelected.Invoke(_sharedOutputDeviceIndexMap[dropdownIndex]);
    }
    
    private void SetInputDevice(int dropdownIndex)
    {
        generalSettings.InputDeviceIndex = _sharedInputDeviceIndexMap[dropdownIndex];

        int inputChannelsCount = DeviceDriver.GetDeviceChannelCount(dropdownIndex, DeviceType.Input);
        inputChannelCountText.text = $"Channels: {inputChannelsCount}";
        
        OnInputDeviceSelected.Invoke(_sharedInputDeviceIndexMap[dropdownIndex], inputChannelsCount);
    }
    
    private void CreateDeviceOptionsFor(TMP_Dropdown deviceDropdown, DeviceType deviceType, int deviceCount)
    {
        switch (deviceType)
        {
            case DeviceType.Input:
                _sharedInputDeviceIndexMap = Enumerable.Range(0, deviceCount)
                    .Where(i => DeviceDriver.IsDeviceRaw(i, deviceType) == false)
                    .ToList();
                break;
            case DeviceType.Output:
                _sharedOutputDeviceIndexMap = Enumerable.Range(0, deviceCount)
                    .Where(i => DeviceDriver.IsDeviceRaw(i, deviceType) == false)
                    .ToList();
                break;
        }

        var deviceIndexMap = deviceType == DeviceType.Input ? _sharedInputDeviceIndexMap : _sharedOutputDeviceIndexMap;

        deviceDropdown.options = deviceIndexMap
            .Select(i => DeviceDriver.GetDeviceName(i, deviceType))
            .Select(deviceName => new TMP_Dropdown.OptionData(text: deviceName))
            .ToList();
        
        deviceDropdown.RefreshShownValue();
    }
}
