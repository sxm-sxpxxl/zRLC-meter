using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ChannelsCalibrator : MonoBehaviour
{
    public event Action<float> OnCalibrationFinished = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;

    [Header("Settings")]
    [SerializeField, Range(1, 50)] private int iterationNumber = 10;
    [SerializeField, Min(0f)] private float calibrationFrequency = 500f;

    private float? _calibrationRatioRms = null;

    public float CalibrationRatioRms => _calibrationRatioRms ?? 1f;

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
        
        _calibrationRatioRms = 0f;
        float channelDifferenceLevel = 0f;
        
        for (int i = 0; i < iterationNumber;)
        {
            var inputRms = inputDeviceListener.InputFilledDataSamples.Rms();
            var outputRms = inputDeviceListener.OutputFilledDataSamples.Rms();

            if (float.IsNaN(inputRms) || float.IsNaN(outputRms))
            {
                yield return null;
                continue;
            }
            
            Debug.Log($"<color=yellow>[it: {i}]</color> Input.Rms: {inputRms} V | Output.Rms: {outputRms} V");
            
            _calibrationRatioRms += outputRms / inputRms;
            channelDifferenceLevel += outputRms.Level() - inputRms.Level();
            i++;
            
            yield return null;
        }
        
        _calibrationRatioRms = Mathf.Abs(_calibrationRatioRms.Value / iterationNumber);
        Debug.Log($"Calibration ratio <color=yellow>RMS</color>: {_calibrationRatioRms.Value} V");
        
        channelDifferenceLevel = Mathf.Abs(channelDifferenceLevel / iterationNumber);
        Debug.Log($"Channel difference <color=green>Level</color>: {channelDifferenceLevel} dBFS");
        
        OnCalibrationFinished.Invoke(channelDifferenceLevel);
        
        outputDeviceGenerator.StopGeneration();
        inputDeviceListener.StopListening();
    }
}
