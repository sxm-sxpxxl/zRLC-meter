using System;
using UnityEngine;
using SoundIO.SimpleDriver;
using Unity.Collections;

public sealed class InputDeviceListener : SingletonBehaviour<InputDeviceListener>
{
    private InputStream _stream;
    private NativeArray<float> _inputDataSamples, _outputDataSamples;
    private int _dataFilledCount, _inputChannelOffset, _outputChannelOffset;
    
    public ReadOnlySpan<float> InputFilledDataSamples => _inputDataSamples.GetSubArray(0, _dataFilledCount).GetReadOnlySpan();
    public ReadOnlySpan<float> OutputFilledDataSamples => _outputDataSamples.GetSubArray(0, _dataFilledCount).GetReadOnlySpan();

    private bool IsListening => _stream != null;

    protected override void Init()
    {
        base.Init();
        _inputDataSamples = new NativeArray<float>(4096, Allocator.Persistent);
        _outputDataSamples = new NativeArray<float>(4096, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        _stream?.Dispose();
        _inputDataSamples.Dispose();
        _outputDataSamples.Dispose();
    }

    private void Update()
    {
        if (IsListening == false)
        {
            return;
        }

        ReadOnlySpan<float> lastFrameWindow = _stream.LastFrameWindow;
        int channelCount = _stream.ChannelCount;

        _dataFilledCount = Mathf.Min(lastFrameWindow.Length, lastFrameWindow.Length / channelCount);

        for (int i = 0; i < _dataFilledCount; i++)
        {
            _inputDataSamples[i] = lastFrameWindow[i * channelCount + _inputChannelOffset];
            _outputDataSamples[i] = lastFrameWindow[i * channelCount + _outputChannelOffset];
        }
    }

    public void StartListening(int inputDeviceIndex, (int, int) inputOutputChannelOffsets)
    {
        if (IsListening)
        {
            Debug.LogWarning("The input device listener is already running!");
            return;
        }

        try
        {
            _stream = DeviceDriver.OpenInputStream(inputDeviceIndex);
        }
        catch (InvalidOperationException error)
        {
            Debug.LogError(error.Message);
            return;
        }

        (_inputChannelOffset, _outputChannelOffset) = inputOutputChannelOffsets;
    }

    public void StopListening()
    {
        if (IsListening == false)
        {
            return;
        }
        
        _stream.Dispose();
        _stream = null;
        _dataFilledCount = 0;
    }
}
