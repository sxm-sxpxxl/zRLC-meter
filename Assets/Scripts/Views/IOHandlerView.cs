using System.Collections.Generic;
using System.Linq;
using SoundIO.SimpleDriver;
using UnityEngine;
using UnityEngine.UI;
using DeviceType = SoundIO.SimpleDriver.DeviceType;

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
    
    private void Start()
    {
        CreateDeviceOptionsFor(outputDeviceDropdown, DeviceType.Output, DeviceDriver.OutputDeviceCount);
        outputDeviceDropdown.onValueChanged.AddListener(SetOutputDevice);
        outputDeviceDropdown.value = DeviceDriver.DefaultOutputDeviceIndex;
        SetOutputDevice(outputDeviceDropdown.value);

        CreateOutputVolumeOptionsFor(outputVolumeDropdown);
        outputVolumeDropdown.onValueChanged.AddListener(SetOutputDeviceVolume);
        SetOutputDeviceVolume(outputVolumeDropdown.value);
        
        CreateDeviceOptionsFor(inputDeviceDropdown, DeviceType.Input, DeviceDriver.InputDeviceCount);
        inputDeviceDropdown.onValueChanged.AddListener(SetInputDevice);
        inputDeviceDropdown.value = DeviceDriver.DefaultInputDeviceIndex;
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

    private void SetOutputDevice(int deviceIndex)
    {
        generalSettings.OutputDeviceIndex = deviceIndex;
    }
    
    private void SetOutputDeviceVolume(int outputDeviceVolumeIndex)
    {
        generalSettings.OutputDeviceVolume = _outputDeviceVolumes[outputDeviceVolumeIndex].InverseLevel(refLevel: 1f);
    }
    
    private void SetInputDevice(int deviceIndex)
    {
        generalSettings.InputDeviceIndex = deviceIndex;

        int inputChannelCount = DeviceDriver.GetDeviceChannelCount(deviceIndex, DeviceType.Input);
        inputChannelCountText.text = inputChannelCount.ToString();
        inputChannelCountText.color = inputChannelCount == 2 ? Color.green : Color.red;
    }
    
    private void SetInputReferencePoint(int inputReferencePoint)
    {
        generalSettings.InputChannelReferencePoint = (ReferencePoint) inputReferencePoint;
    }
    
    private void CreateDeviceOptionsFor(Dropdown deviceDropdown, DeviceType deviceType, int deviceCount)
    {
        deviceDropdown.options = Enumerable.Range(0, deviceCount)
            // TODO: fix it
            // .Where(i => DeviceDriver.IsDeviceRaw(i, deviceType) == false)
            .Select(i => DeviceDriver.GetDeviceName(i, deviceType))
            .Select(name => new Dropdown.OptionData(text: name))
            .ToList();
        
        deviceDropdown.RefreshShownValue();
    }
}
