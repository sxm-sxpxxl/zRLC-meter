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
        outputDeviceDropdown.onValueChanged.AddListener(SetOutputDevice);
        outputDeviceDropdown.value = _sharedOutputDeviceIndexMap.FindIndex(0, i => i == DeviceDriver.DefaultOutputDeviceIndex);
        SetOutputDevice(outputDeviceDropdown.value);

        CreateDeviceOptionsFor(inputDeviceDropdown, DeviceType.Input, DeviceDriver.InputDeviceCount);
        inputDeviceDropdown.onValueChanged.AddListener(SetInputDevice);
        inputDeviceDropdown.value = _sharedOutputDeviceIndexMap.FindIndex(0, i => i == DeviceDriver.DefaultInputDeviceIndex);
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
    }
    
    private void SetInputDevice(int dropdownIndex)
    {
        generalSettings.InputDeviceIndex = _sharedInputDeviceIndexMap[dropdownIndex];

        int inputChannelCount = DeviceDriver.GetDeviceChannelCount(dropdownIndex, DeviceType.Input);
        inputChannelCountText.text = $"Channels: {inputChannelCount}";
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
