using UnityEngine;
using SoundIO.SimpleDriver;

public sealed class SoundcardSetupPageValidator : PageValidator
{
    [Header("Dependencies")]
    [SerializeField] private SoundcardSetupView soundcardSetupView;
    [SerializeField, Range(1, 3)] private int requiredChannelsCount = 2;

    [Header("Validation info boxes")]
    [SerializeField] private GameObject noneInputDeviceInfoBox;
    [SerializeField] private GameObject wrongChannelsInfoBox;
    [SerializeField] private GameObject noneOutputDeviceInfoBox;

    private bool _isInputDeviceValid, _isOutputDeviceValid;
    
    protected override void Init()
    {
        base.Init();
        
        noneInputDeviceInfoBox.SetActive(true);
        wrongChannelsInfoBox.SetActive(false);
        noneOutputDeviceInfoBox.SetActive(true);

        _isInputDeviceValid = _isOutputDeviceValid = false;
        UpdateValidationState();
        
        soundcardSetupView.OnInputDeviceSelected += OnInputDeviceSelected;
        soundcardSetupView.OnOutputDeviceSelected += OnOutputDeviceSelected;
    }

    protected override void Deinit()
    {
        base.Deinit();
        soundcardSetupView.OnInputDeviceSelected -= OnInputDeviceSelected;
        soundcardSetupView.OnOutputDeviceSelected -= OnOutputDeviceSelected;
    }

    private void OnInputDeviceSelected(int deviceIndex, int channelsCount)
    {
        bool isDeviceSelected = deviceIndex >= 0 && deviceIndex < DeviceDriver.InputDeviceCount;
        noneInputDeviceInfoBox.SetActive(isDeviceSelected == false);

        if (isDeviceSelected == false)
        {
            _isInputDeviceValid = false;
            UpdateValidationState();
            return;
        }

        bool isChannelsCountValid = channelsCount == requiredChannelsCount;
        wrongChannelsInfoBox.SetActive(isChannelsCountValid == false);

        _isInputDeviceValid = isChannelsCountValid;
        UpdateValidationState();
    }

    private void OnOutputDeviceSelected(int deviceIndex)
    {
        _isOutputDeviceValid = deviceIndex >= 0 && deviceIndex < DeviceDriver.OutputDeviceCount;
        noneOutputDeviceInfoBox.SetActive(_isOutputDeviceValid == false);
        UpdateValidationState();
    }

    private void UpdateValidationState()
    {
        IsValid = _isInputDeviceValid && _isOutputDeviceValid;
    }
}
