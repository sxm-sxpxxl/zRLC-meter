using System;
using UnityEngine;
using SoundIO.SimpleDriver;
using Unity.Collections;

/// <summary>
/// Устанавливает соединение с входным аудио-устройством по запросу и продолжает слушать и собирать данные со входа,
/// пока не поступит запрос о прекращения прослушивания.
/// 
/// ! Взаимодействует с API аудио-библиотеки libsoundio !
/// </summary>
public sealed class InputDeviceListener : SingletonBehaviour<InputDeviceListener>
{
    private const int DataSamplesLength = 4096;
    
    private InputStream _stream;
    private NativeArray<float> _inputDataSamples, _outputDataSamples;
    private int _lastDataFilledIndex, _inputChannelOffset, _outputChannelOffset;
    
    public int SampleRate => _stream.SampleRate;
    
    private bool IsListening => _stream != null;

    protected override void Init()
    {
        base.Init();
        _inputDataSamples = new NativeArray<float>(DataSamplesLength, Allocator.Persistent);
        _outputDataSamples = new NativeArray<float>(DataSamplesLength, Allocator.Persistent);
        _lastDataFilledIndex = 0;
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
        
        int lastFrameDataFilledCount = Mathf.Min(lastFrameWindow.Length, lastFrameWindow.Length / channelCount);
        if (lastFrameDataFilledCount == 0)
        {
            return;
        }
        
        int nextDataFilledCount = _lastDataFilledIndex + lastFrameDataFilledCount;
        if (nextDataFilledCount > DataSamplesLength)
        {
            _lastDataFilledIndex = 0;
            nextDataFilledCount = lastFrameDataFilledCount;
        }
        
        for (int i = 0; i < lastFrameDataFilledCount; i++)
        {
            _inputDataSamples[_lastDataFilledIndex + i] = lastFrameWindow[i * channelCount + _inputChannelOffset];
            _outputDataSamples[_lastDataFilledIndex + i] = lastFrameWindow[i * channelCount + _outputChannelOffset];
        }
        
        _lastDataFilledIndex = nextDataFilledCount - 1;
    }

    public bool TryGetAndReleaseFilledSamplesByIntervals(
        float frequency,
        int intervalsCount,
        out ReadOnlySpan<float> inputFilledSamplesByIntervals,
        out ReadOnlySpan<float> inputShiftFilledSamplesByIntervals,
        out ReadOnlySpan<float> outputFilledSamplesByIntervals
    )
    {
        int samplesCountPerInterval = Mathf.CeilToInt(SampleRate / frequency);
        int samplesCountPerQuarterInterval = Mathf.CeilToInt(samplesCountPerInterval / 4f);
        int totalSamplesCount = samplesCountPerInterval * intervalsCount;

        if (_lastDataFilledIndex < totalSamplesCount)
        {
            inputFilledSamplesByIntervals = inputShiftFilledSamplesByIntervals = outputFilledSamplesByIntervals = null;
            return false;
        }

        _lastDataFilledIndex = 0;
        
        inputFilledSamplesByIntervals = _inputDataSamples.GetSubArray(0, totalSamplesCount).GetReadOnlySpan();
        inputShiftFilledSamplesByIntervals = _inputDataSamples.GetSubArray(samplesCountPerQuarterInterval, totalSamplesCount).GetReadOnlySpan();
        outputFilledSamplesByIntervals = _outputDataSamples.GetSubArray(0, totalSamplesCount).GetReadOnlySpan();
        return true;
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
        _lastDataFilledIndex = 0;
    }
}
