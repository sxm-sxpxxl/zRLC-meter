using SoundIO.SimpleDriver;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DeviceType = SoundIO.SimpleDriver.DeviceType;

// Output device selector class
// - Controlls the output device selection UI.
// - Manages SoundIO objects.

public sealed class OutputDeviceSelector : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Dropdown _deviceList = null;
    [SerializeField] Dropdown _channelList = null;
    [SerializeField] Text _statusText = null;
    [SerializeField, Min(0)] int _sampleRate = 44100;

    #endregion
    
    #region Public properties

    public int Channel => _channelList.value;
    public int ChannelCount => _stream?.ChannelCount ?? 0;
    public int SampleRate => _stream?.SampleRate ?? 0;

    public float Volume { get; set; } = 1;

    #endregion
    
    #region Internal objects

    OutputStream _stream;

    #endregion
    
    #region MonoBehaviour implementation

    void Start()
    {
        // Clear the UI contents.
        _deviceList.ClearOptions();
        _channelList.ClearOptions();
        _statusText.text = "";

        // Null device option
        _deviceList.options.Add(new Dropdown.OptionData() { text = "--" });

        // Device list initialization
        _deviceList.options.AddRange(
            Enumerable.Range(0, DeviceDriver.OutputDeviceCount).
                Select(i => DeviceDriver.GetDeviceName(i, DeviceType.Output)).
                Select(name => new Dropdown.OptionData() { text = name }));

        _deviceList.RefreshShownValue();
    }

    void OnDestroy()
    {
        _stream?.Dispose();
    }

    void Update()
    {
        if (_stream == null)
        {
            return;
        }

        _stream.SetVolume(Volume);

        // Status line
        _statusText.text =
            $"Sampling rate: {_stream.SampleRate:n0}Hz / " +
            $"Software Latency: {_stream.Latency * 1000:n2}ms / " +
            $"Amplifier: {Volume:P0}";
    }

    #endregion
    
    #region UI callback

    public void OnDeviceSelected(int index)
    {
        // Stop and destroy the current stream.
        if (_stream != null)
        {
            _stream.Dispose();
            _stream = null;
        }

        // Reset the UI elements.
        _channelList.ClearOptions();
        _channelList.RefreshShownValue();
        _statusText.text = "";

        // Break if the null device option was selected.
        if (_deviceList.value == 0) return;

        // Open a new stream.
        try
        {
            _stream = DeviceDriver.OpenOutputStream(_deviceList.value - 1, _sampleRate, OnSampleRead);
        }
        catch (System.InvalidOperationException e)
        {
            _statusText.text = $"Error: {e.Message}";
            return;
        }

        // Construct the channel list.
        _channelList.options =
            Enumerable.Range(0, _stream.ChannelCount).
                Select(i => $"Channel {i + 1}").
                Select(text => new Dropdown.OptionData() { text = text }).
                ToList();

        _channelList.RefreshShownValue();
    }

    private double OnSampleRead(int frameIndex, double offsetInSeconds, double secondsPerFrame)
    {
        double frequency = 100d;
        double radiansPerSecond = 2d * Math.PI * frequency;
        
        return Math.Sin((offsetInSeconds + frameIndex * secondsPerFrame) * radiansPerSecond);
    }

    #endregion
}
