using System;
using UnityEngine;
using SoundIO.SimpleDriver;

/// <summary>
/// Устанавливает соединение с выходным аудио-устройством по запросу и продолжает генерировать на выход семплы
/// синусоидального сигнала с параметрами заданными в основных настройках, пока не поступит запрос о прекращении генерации.
/// </summary>
public sealed class OutputDeviceGenerator : SingletonBehaviour<OutputDeviceGenerator>
{
    [SerializeField] private GeneralSettings generalSettings;
    
    private OutputStream _stream;
    private double _angularFrequency;
    
    private bool IsGenerating => _stream != null;

    private void OnDestroy()
    {
        _stream?.Dispose();
    }

    public void StartGeneration(int outputDeviceIndex, float waveFrequency, int sampleRate)
    {
        if (IsGenerating)
        {
            Debug.LogWarning("The output device generator is already running!");
            return;
        }

        try
        {
            _stream = DeviceDriver.OpenOutputStream(outputDeviceIndex, sampleRate, OnSampleRead);
            _stream.SetVolume(generalSettings.OutputDeviceVolume);
        }
        catch (InvalidOperationException error)
        {
            Debug.LogError(error.Message);
            return;
        }

        _angularFrequency = 2d * Math.PI * waveFrequency;
    }

    public void StopGeneration()
    {
        if (IsGenerating == false)
        {
            return;
        }

        _stream.Dispose();
        _stream = null;
    }

    private double OnSampleRead(int frameIndex, double offsetInSeconds, double secondsPerFrame)
        => Math.Sin((offsetInSeconds + frameIndex * secondsPerFrame) * _angularFrequency);
}
