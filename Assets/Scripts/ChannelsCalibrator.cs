using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Отвечает за калибровку левого и правого каналов входного порта Line In.
/// </summary>
[DisallowMultipleComponent]
public sealed class ChannelsCalibrator : MonoBehaviour
{
    public event Action<float> OnCalibrationFinished = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;

    [Header("Settings")]
    [SerializeField, Range(1, 100)] private int iterationNumber = 10;
    [SerializeField, Min(0f)] private float calibrationFrequency = 500f;

    private float? _calibrationMagnitudeRatioRms = null;

    public float CalibrationMagnitudeRatioRms => _calibrationMagnitudeRatioRms ?? 1f;

    public void Calibrate()
    {
        StartCoroutine(CalibrationCoroutine());
    }

    private IEnumerator CalibrationCoroutine()
    {
        var outputDeviceGenerator = OutputDeviceGenerator.Instance;
        var inputDeviceListener = InputDeviceListener.Instance;
        
        outputDeviceGenerator.StartGeneration(generalSettings.OutputDeviceIndex, calibrationFrequency, generalSettings.SampleRate);
        inputDeviceListener.StartListening(generalSettings.InputDeviceIndex, generalSettings.InputOutputChannelOffsets);

        yield return new WaitForSecondsRealtime(generalSettings.TransientTimeInMs.ConvertToNormal(fromMetric: Metric.Milli));
        
        _calibrationMagnitudeRatioRms = 0f;
        float channelDifferenceLevel = 0f;
        
        for (int i = 0; i < iterationNumber;)
        {
            if (inputDeviceListener.TryGetAndReleaseFilledSamplesByIntervals(
                frequency: calibrationFrequency,
                intervalsCount: 10,
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
                yield return null;
                continue;
            }
            
            Debug.Log($"<color=yellow>[it: {i}]</color> Input.Rms: {inputRms} V | Output.Rms: {outputRms} V");
            
            _calibrationMagnitudeRatioRms += outputRms / inputRms;
            channelDifferenceLevel += outputRms.Level() - inputRms.Level();
            i++;
            
            yield return null;
        }
        
        _calibrationMagnitudeRatioRms = Mathf.Abs(_calibrationMagnitudeRatioRms.Value / iterationNumber);
        Debug.Log($"Calibration <color=yellow>Magnitude RMS</color>: {_calibrationMagnitudeRatioRms.Value} V");
        
        channelDifferenceLevel = Mathf.Abs(channelDifferenceLevel / iterationNumber);
        Debug.Log($"Channel <color=yellow>Difference Level</color>: {channelDifferenceLevel} dBFS");
        
        OnCalibrationFinished.Invoke(channelDifferenceLevel);
        
        outputDeviceGenerator.StopGeneration();
        inputDeviceListener.StopListening();
    }
}
