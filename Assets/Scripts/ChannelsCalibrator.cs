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
    [SerializeField, Range(1, 100)] private int iterationNumber = 10;
    [SerializeField, Min(0f)] private float calibrationFrequency = 500f;

    private float? _calibrationMagnitudeRatioRms = null;
    private float? _calibrationPhaseShiftInDeg = null;

    public float CalibrationMagnitudeRatioRms => _calibrationMagnitudeRatioRms ?? 1f;
    public float CalibrationPhaseShiftInDeg => _calibrationPhaseShiftInDeg ?? 0f;

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
        
        _calibrationMagnitudeRatioRms = _calibrationPhaseShiftInDeg = 0f;
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
            
            _calibrationMagnitudeRatioRms += outputRms / inputRms;
            _calibrationPhaseShiftInDeg += ZRLCHelper.ComputePhaseShift(
                inputDeviceListener.InputFilledDataSamples,
                inputDeviceListener.OutputFilledDataSamples,
                inputDeviceListener.SampleRate,
                calibrationFrequency,
                outputRms / inputRms
            );
            channelDifferenceLevel += outputRms.Level() - inputRms.Level();
            i++;
            
            yield return null;
        }
        
        _calibrationMagnitudeRatioRms = Mathf.Abs(_calibrationMagnitudeRatioRms.Value / iterationNumber);
        _calibrationPhaseShiftInDeg = _calibrationPhaseShiftInDeg.Value / iterationNumber;
        Debug.Log($"Calibration <color=yellow>magnitude RMS</color>: {_calibrationMagnitudeRatioRms.Value} V | Calibration <color=yellow>phase</color>: {_calibrationPhaseShiftInDeg}°");
        
        channelDifferenceLevel = Mathf.Abs(channelDifferenceLevel / iterationNumber);
        Debug.Log($"Channel difference <color=green>Level</color>: {channelDifferenceLevel} dBFS");
        
        OnCalibrationFinished.Invoke(channelDifferenceLevel);
        
        outputDeviceGenerator.StopGeneration();
        inputDeviceListener.StopListening();
    }
}
