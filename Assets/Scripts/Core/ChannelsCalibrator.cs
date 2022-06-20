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
    public event Action<ComplexFloat> OnGainCalibrationFinished = delegate { };
    public event Action<ComplexFloat> OnOpenCalibrationFinished = delegate { };
    public event Action<ComplexFloat> OnShortCalibrationFinished = delegate { };
    public event Action<string> OnCalibrationErrorOccurred = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;

    private OutputDeviceGenerator _outputDeviceGenerator;
    private InputDeviceListener _inputDeviceListener;

    private ComplexFloat? _gainCorrectionRatio, _lineInputImpedance, _groundImpedance;

    public ComplexFloat GainCorrectionRatio => _gainCorrectionRatio ?? ComplexFloat.FromAngle(0f, 1f);
    public ComplexFloat LineInputImpedance => _lineInputImpedance ?? ComplexFloat.FromAngle(0f, 1e9f);
    public ComplexFloat GroundImpedance => _groundImpedance ?? ComplexFloat.FromAngle(0f, 0f);

    private void Start()
    {
        _outputDeviceGenerator = OutputDeviceGenerator.Instance;
        _inputDeviceListener = InputDeviceListener.Instance;

        Assert.IsNotNull(_outputDeviceGenerator);
        Assert.IsNotNull(_inputDeviceListener);
    }

    public void GainCalibrate()
    {
        StartCoroutine(GainCorrectionCoroutine(result =>
        {
            _gainCorrectionRatio = result;
            Debug.Log(
                $"<color=yellow>Gain correction ratio (In/Out): </color>{_gainCorrectionRatio.Value.real} + {_gainCorrectionRatio.Value.imag}j  " +
                $"<color=yellow>Magnitude:</color> {_gainCorrectionRatio.Value.Magnitude}  " +
                $"<color=yellow>Phase:</color> {_gainCorrectionRatio.Value.AngleInRad * Mathf.Rad2Deg} °");

            OnGainCalibrationFinished.Invoke(_gainCorrectionRatio.Value);
        }));
    }

    public void OpenCalibrate()
    {
        StartCoroutine(CalibrationCoroutine(result =>
        {
            _lineInputImpedance = result;
            Debug.Log($"LineIn impedance: <color=yellow>{_lineInputImpedance.Value.real} + {_lineInputImpedance.Value.imag}j</color>  " +
                      $"<color=yellow>Magnitude</color> = {_lineInputImpedance.Value.Magnitude} Ohm  " +
                      $"<color=yellow>Phase</color> = {_lineInputImpedance.Value.AngleInRad * Mathf.Rad2Deg} °");

            OnOpenCalibrationFinished.Invoke(_lineInputImpedance.Value);
        }));
    }
    
    public void ShortCalibrate()
    {
        StartCoroutine(CalibrationCoroutine(result =>
        {
            _groundImpedance = result.AsReal;
            Debug.Log($"<color=yellow>Ground impedance:</color> {_groundImpedance.Value.real} + {_groundImpedance.Value.imag}j  " +
                      $"<color=yellow>Magnitude</color> = {_groundImpedance.Value.Magnitude} Ohm  " +
                      $"<color=yellow>Phase</color> = {_groundImpedance.Value.AngleInRad * Mathf.Rad2Deg} °");
            
            OnShortCalibrationFinished.Invoke(_groundImpedance.Value);
        }));
    }

    private IEnumerator GainCorrectionCoroutine(Action<ComplexFloat> getGainCorrectionRatioCallback)
    {
        _outputDeviceGenerator.StartGeneration(generalSettings.OutputDeviceIndex, generalSettings.CalibrationFrequency, generalSettings.SamplingRate);
        _inputDeviceListener.StartListening(generalSettings.InputDeviceIndex, generalSettings.InputOutputChannelOffsets);

        yield return new WaitForSecondsRealtime(generalSettings.TransientTimeInMs.ConvertToNormal(fromMetric: Metric.Milli));
        
        ComplexFloat gainCorrectionRatio = ComplexFloat.Zero;
        float rmsRatio = 0f;

        for (int i = 0; i < generalSettings.AveragingIterations;)
        {
            if (_inputDeviceListener.TryGetAndReleaseFilledSamplesByIntervals(
                generalSettings.CalibrationFrequency,
                generalSettings.SignalIntervalsCount,
                out ReadOnlySpan<float> inputDataSamples,
                out ReadOnlySpan<float> outputDataSamples
            ) == false)
            {
                yield return null;
                continue;
            }

            ComplexFloat inPeak = inputDataSamples.ComplexPeak(generalSettings.CalibrationFrequency, generalSettings.SamplingRate);
            ComplexFloat outPeak = outputDataSamples.ComplexPeak(generalSettings.CalibrationFrequency, generalSettings.SamplingRate);
            
            gainCorrectionRatio += inPeak / outPeak;
            rmsRatio += inputDataSamples.Rms() / outputDataSamples.Rms();
            i++;

            yield return null;
        }
        
        _outputDeviceGenerator.StopGeneration();
        _inputDeviceListener.StopListening();

        rmsRatio /= generalSettings.AveragingIterations;
        Debug.Log($"<color=yellow>In/Out RMS ratio:</color> {rmsRatio} V");

        gainCorrectionRatio /= generalSettings.AveragingIterations;
        getGainCorrectionRatioCallback.Invoke(gainCorrectionRatio);
    }

    private IEnumerator CalibrationCoroutine(Action<ComplexFloat> getCalibratedImpedanceCallback)
    {
        _outputDeviceGenerator.StartGeneration(generalSettings.OutputDeviceIndex, generalSettings.CalibrationFrequency, generalSettings.SamplingRate);
        _inputDeviceListener.StartListening(generalSettings.InputDeviceIndex, generalSettings.InputOutputChannelOffsets);

        yield return new WaitForSecondsRealtime(generalSettings.TransientTimeInMs.ConvertToNormal(fromMetric: Metric.Milli));
        
        ComplexFloat calibratedImpedance = ComplexFloat.Zero;
        float elapsedRetryTimeoutInSec = 0f, inputChannelRms = 0f, outputChannelRms = 0f;
        
        for (int i = 0; i < generalSettings.AveragingIterations;)
        {
            if (_inputDeviceListener.TryGetAndReleaseFilledSamplesByIntervals(
                generalSettings.CalibrationFrequency,
                generalSettings.SignalIntervalsCount,
                out ReadOnlySpan<float> inputDataSamples,
                out ReadOnlySpan<float> outputDataSamples
            ) == false)
            {
                yield return null;
                continue;
            }

            ComplexFloat computedImpedance = ZRLCHelper.ComputeImpedance(
                inputDataSamples,
                outputDataSamples,
                generalSettings.ReferenceResistance,
                generalSettings.CalibrationFrequency,
                generalSettings.SamplingRate
            );

            if (float.IsNaN(computedImpedance.Magnitude) || float.IsNaN(computedImpedance.AngleInRad))
            {
                if (elapsedRetryTimeoutInSec > generalSettings.RetryTimeoutInSec)
                {
                    _outputDeviceGenerator.StopGeneration();
                    _inputDeviceListener.StopListening();
                    
                    OnCalibrationErrorOccurred.Invoke(
                        "The impedance was measured as NaN over an extended period of time. " +
                            "Сheck your circuit, its connection to the soundcard and try again."
                    );
                    yield break;
                }
                
                elapsedRetryTimeoutInSec += Time.deltaTime;
                yield return null;
                continue;
            }

            inputChannelRms += inputDataSamples.Rms();
            outputChannelRms += outputDataSamples.Rms();
            
            calibratedImpedance += computedImpedance;
            i++;
            
            elapsedRetryTimeoutInSec = 0f;
            yield return null;
        }
        
        _outputDeviceGenerator.StopGeneration();
        _inputDeviceListener.StopListening();

        inputChannelRms /= generalSettings.AveragingIterations;
        outputChannelRms /= generalSettings.AveragingIterations;
        
        Debug.Log($"<color=yellow>Input channel RMS: </color> {inputChannelRms} V  " +
                  $"<color=yellow>Output channel RMS: </color> {outputChannelRms} V");
        
        calibratedImpedance /= generalSettings.AveragingIterations;
        getCalibratedImpedanceCallback.Invoke(calibratedImpedance);
    }
}
