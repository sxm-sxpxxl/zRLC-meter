using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Отвечает за калибровку левого и правого каналов входного порта Line In.
/// </summary>
[DisallowMultipleComponent]
public sealed class ChannelsCalibrator : MonoBehaviour
{
    private const float MaxChannelDifferenceLevel = 3f;
    
    public event Action<float> OnCalibrationFinished = delegate { };
    public event Action<string> OnCalibrationErrorOccurred = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;

    [Header("Settings")]
    [SerializeField, Range(1, 100)] private int iterationNumber = 10;
    [SerializeField, Min(0f)] private float calibrationFrequency = 500f;

    private float? _calibrationMagnitudeRatioRms = null;
    private OutputDeviceGenerator _outputDeviceGenerator;
    private InputDeviceListener _inputDeviceListener;

    public float CalibrationMagnitudeRatioRms => _calibrationMagnitudeRatioRms ?? 1f;
    
    private bool IsChannelCountValid => _inputDeviceListener.GetChannelCountBy(generalSettings.InputDeviceIndex) == 2;

    private void Start()
    {
        _outputDeviceGenerator = OutputDeviceGenerator.Instance;
        _inputDeviceListener = InputDeviceListener.Instance;

        Assert.IsNotNull(_outputDeviceGenerator);
        Assert.IsNotNull(_inputDeviceListener);
    }

    public void Calibrate()
    {
        StartCoroutine(CalibrationCoroutine());
    }

    private IEnumerator CalibrationCoroutine()
    {
        var outputDeviceGenerator = OutputDeviceGenerator.Instance;
        var inputDeviceListener = InputDeviceListener.Instance;
        
        if (IsChannelCountValid == false)
        {
            OnCalibrationErrorOccurred.Invoke("There must be two channels of LineIn to carry out measurements. Сheck your connections and try again.");
            yield break;
        }
        
        outputDeviceGenerator.StartGeneration(generalSettings.OutputDeviceIndex, calibrationFrequency, generalSettings.SampleRate);
        inputDeviceListener.StartListening(generalSettings.InputDeviceIndex, generalSettings.InputOutputChannelOffsets);

        yield return new WaitForSecondsRealtime(generalSettings.TransientTimeInMs.ConvertToNormal(fromMetric: Metric.Milli));
        
        _calibrationMagnitudeRatioRms = 0f;
        float channelDifferenceLevel = 0f, elapsedRetryTimeoutInSec = 0f;
        
        for (int i = 0; i < iterationNumber;)
        {
            if (inputDeviceListener.TryGetAndReleaseFilledSamplesByIntervals(
                frequency: calibrationFrequency,
                intervalsCount: 1,
                out ReadOnlySpan<float> inputDataSamples,
                out ReadOnlySpan<float> inputShiftDataSamples,
                out ReadOnlySpan<float> outputDataSamples
            ) == false)
            {
                yield return null;
                continue;
            }
            
            var inputRms = inputDataSamples.Rms();
            var outputRms = outputDataSamples.Rms();

            if (float.IsNaN(inputRms) || float.IsNaN(outputRms))
            {
                if (elapsedRetryTimeoutInSec > generalSettings.RetryTimeoutInSec)
                {
                    outputDeviceGenerator.StopGeneration();
                    inputDeviceListener.StopListening();
                    
                    OnCalibrationErrorOccurred.Invoke("I/O channels are measured as NaN. Сheck your circuit and try again.");
                    yield break;
                }
                
                elapsedRetryTimeoutInSec += Time.deltaTime;
                yield return null;
                continue;
            }
            
            Debug.Log($"<color=yellow>[it: {i}]</color> Input.Rms: {inputRms} V | Output.Rms: {outputRms} V");
            
            _calibrationMagnitudeRatioRms += outputRms / inputRms;
            channelDifferenceLevel += outputRms.Level() - inputRms.Level();
            i++;
            
            elapsedRetryTimeoutInSec = 0f;
            yield return null;
        }
        
        outputDeviceGenerator.StopGeneration();
        inputDeviceListener.StopListening();
        
        _calibrationMagnitudeRatioRms = Mathf.Abs(_calibrationMagnitudeRatioRms.Value / iterationNumber);
        Debug.Log($"Calibration <color=yellow>Magnitude RMS</color>: {_calibrationMagnitudeRatioRms.Value} V");
        
        channelDifferenceLevel = Mathf.Abs(channelDifferenceLevel / iterationNumber);
        Debug.Log($"Channel <color=yellow>Difference Level</color>: {channelDifferenceLevel} dBFS");
        
        OnCalibrationFinished.Invoke(channelDifferenceLevel);

        if (channelDifferenceLevel > MaxChannelDifferenceLevel)
        {
            OnCalibrationErrorOccurred.Invoke("Channel difference level larger than 3 dB. Check your connections and try again.");
        }
    }
}
