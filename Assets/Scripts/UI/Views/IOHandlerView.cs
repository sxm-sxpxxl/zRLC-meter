using System;
using System.Collections.Generic;
using System.Linq;
using SoundIO.SimpleDriver;
using UnityEngine;
using UnityEngine.UI;
using DeviceType = SoundIO.SimpleDriver.DeviceType;

/// <summary>
/// Обрабатывает UI элементы, настраивающие аудио-устройства ввода-вывода.
/// </summary>
[DisallowMultipleComponent]
public sealed class IOHandlerView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;
    
    [Header("Views")]
    [SerializeField] private Dropdown outputDeviceDropdown;
    [SerializeField] private Dropdown outputVolumeDropdown;
    [SerializeField, Range(1, 80)] private int volumeRangeCount = 12;
    
    [Space]
    [SerializeField] private Dropdown inputDeviceDropdown;
    [SerializeField] private Dropdown inputReferencePointDropdown;
    [SerializeField] private Text inputChannelCountText;

    private List<float> _outputDeviceVolumes;
    private List<int> _sharedInputDeviceIndexMap, _sharedOutputDeviceIndexMap;
    
    private void Start()
    {
        CreateDeviceOptionsFor(outputDeviceDropdown, DeviceType.Output, DeviceDriver.OutputDeviceCount);
        outputDeviceDropdown.onValueChanged.AddListener(SetOutputDevice);
        outputDeviceDropdown.value = _sharedOutputDeviceIndexMap.FindIndex(0, i => i == DeviceDriver.DefaultOutputDeviceIndex);
        SetOutputDevice(outputDeviceDropdown.value);

        CreateOutputVolumeOptionsFor(outputVolumeDropdown);
        outputVolumeDropdown.onValueChanged.AddListener(SetOutputDeviceVolume);
        SetOutputDeviceVolume(outputVolumeDropdown.value);
        
        CreateDeviceOptionsFor(inputDeviceDropdown, DeviceType.Input, DeviceDriver.InputDeviceCount);
        inputDeviceDropdown.onValueChanged.AddListener(SetInputDevice);
        inputDeviceDropdown.value = _sharedOutputDeviceIndexMap.FindIndex(0, i => i == DeviceDriver.DefaultInputDeviceIndex);
        SetInputDevice(inputDeviceDropdown.value);
        
        inputReferencePointDropdown.onValueChanged.AddListener(SetInputReferencePoint);
    }
    
    private void OnDestroy()
    {
        outputDeviceDropdown.onValueChanged.RemoveListener(SetOutputDevice);
        outputVolumeDropdown.onValueChanged.RemoveListener(SetOutputDeviceVolume);
        inputDeviceDropdown.onValueChanged.RemoveListener(SetInputDevice);
    }

    private void CreateOutputVolumeOptionsFor(Dropdown volumeDropdown)
    {
        _outputDeviceVolumes = Enumerable.Range(0, volumeRangeCount + 1)
            .Select(i => (float)-i)
            .ToList();
        
        volumeDropdown.options = _outputDeviceVolumes
            .Select(i => new Dropdown.OptionData(text: $"{i} dB"))
            .ToList();
        
        volumeDropdown.RefreshShownValue();
    }

    private void SetOutputDevice(int dropdownIndex)
    {
        generalSettings.OutputDeviceIndex = _sharedOutputDeviceIndexMap[dropdownIndex];
    }
    
    private void SetOutputDeviceVolume(int outputDeviceVolumeIndex)
    {
        generalSettings.OutputDeviceVolume = _outputDeviceVolumes[outputDeviceVolumeIndex].InverseLevel(refLevel: 1f);
    }
    
    private void SetInputDevice(int dropdownIndex)
    {
        generalSettings.InputDeviceIndex = _sharedInputDeviceIndexMap[dropdownIndex];

        int inputChannelCount = DeviceDriver.GetDeviceChannelCount(dropdownIndex, DeviceType.Input);
        inputChannelCountText.text = inputChannelCount.ToString();
        inputChannelCountText.color = inputChannelCount == 2 ? Color.green : Color.red;
    }
    
    private void SetInputReferencePoint(int inputReferencePoint)
    {
        generalSettings.InputReferenceChannel = (ReferenceChannel) inputReferencePoint;
    }
    
    private void CreateDeviceOptionsFor(Dropdown deviceDropdown, DeviceType deviceType, int deviceCount)
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
            .Select(name => new Dropdown.OptionData(text: name))
            .ToList();
        
        deviceDropdown.RefreshShownValue();
    }
}
