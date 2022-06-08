﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Отвечает за калибровку левого и правого каналов входного порта Line In.
/// </summary>
[DisallowMultipleComponent]
public sealed class ChannelsCalibrator : MonoBehaviour
{
    public event Action OnCalibrationFinished = delegate { };
    public event Action<string> OnCalibrationErrorOccurred = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;

    private OutputDeviceGenerator _outputDeviceGenerator;
    private InputDeviceListener _inputDeviceListener;

    private ComplexFloat? _lineInputImpedance, _groundImpedance;

    public ComplexFloat LineInputImpedance => _lineInputImpedance ?? ComplexFloat.FromAngle(0f, 1e9f);
    public ComplexFloat GroundImpedance => _groundImpedance ?? ComplexFloat.FromAngle(0f, 0f);

    private bool IsChannelCountValid => _inputDeviceListener.GetChannelCountBy(generalSettings.InputDeviceIndex) == 2;

    private void Start()
    {
        _outputDeviceGenerator = OutputDeviceGenerator.Instance;
        _inputDeviceListener = InputDeviceListener.Instance;

        Assert.IsNotNull(_outputDeviceGenerator);
        Assert.IsNotNull(_inputDeviceListener);
    }

    public void OpenCalibrate()
    {
        const float MinLineInputImpedanceMagnitude = 1e4f;
        
        StartCoroutine(AverageCalibrationCoroutine(result =>
        {
            _lineInputImpedance = result;
            Debug.Log($"LineIn impedance: <color=yellow>{_lineInputImpedance.Value.real} + {_lineInputImpedance.Value.imag}j</color> " +
                      $"| <color=yellow>Magnitude</color> = {_lineInputImpedance.Value.Magnitude} Ohm " +
                      $"| <color=yellow>Phase</color> = {_lineInputImpedance.Value.AngleInRad * Mathf.Rad2Deg} °");

            if (_lineInputImpedance.Value.Magnitude < MinLineInputImpedanceMagnitude)
            {
                OnCalibrationErrorOccurred.Invoke($"LineIn impedance is measured as too small ({_lineInputImpedance.Value.Magnitude} Ohm). Check your circuit and try again.");
                return;
            }
            
            OnCalibrationFinished.Invoke();
        }));
    }
    
    public void ShortCalibrate()
    {
        const float MaxGroundImpedanceMagnitude = 1f;
        
        StartCoroutine(AverageCalibrationCoroutine(result =>
        {
            _groundImpedance = result;
            Debug.Log($"Ground impedance: <color=yellow>{_groundImpedance.Value.real} + {_groundImpedance.Value.imag}j</color> " +
                      $"| <color=yellow>Magnitude</color> = {_groundImpedance.Value.Magnitude} Ohm " +
                      $"| <color=yellow>Phase</color> = {_groundImpedance.Value.AngleInRad * Mathf.Rad2Deg} °");
            
            if (_groundImpedance.Value.Magnitude > MaxGroundImpedanceMagnitude)
            {
                OnCalibrationErrorOccurred.Invoke($"Ground impedance is measured as too big ({_groundImpedance.Value.Magnitude} Ohm). Check your circuit and try again.");
                return;
            }

            OnCalibrationFinished.Invoke();
        }));
    }

    private IEnumerator AverageCalibrationCoroutine(Action<ComplexFloat> getAverageCalibratedImpedanceCallback)
    {
        int iterations = Mathf.CeilToInt(generalSettings.AveragingIterations / 20f);
        ComplexFloat averageCalibratedImpedance = ComplexFloat.Zero;

        for (int i = 0; i < iterations; i++)
        {
            yield return StartCoroutine(CalibrationCoroutine(result => averageCalibratedImpedance += result));
        }

        averageCalibratedImpedance /= iterations;
        getAverageCalibratedImpedanceCallback.Invoke(averageCalibratedImpedance);
    }

    private IEnumerator CalibrationCoroutine(Action<ComplexFloat> getCalibratedImpedanceCallback)
    {
        var outputDeviceGenerator = OutputDeviceGenerator.Instance;
        var inputDeviceListener = InputDeviceListener.Instance;
        
        if (IsChannelCountValid == false)
        {
            OnCalibrationErrorOccurred.Invoke("There must be two channels of LineIn to carry out measurements. Сheck your connections and try again.");
            yield break;
        }
        
        outputDeviceGenerator.StartGeneration(generalSettings.OutputDeviceIndex, generalSettings.CalibrationFrequency, generalSettings.SamplingRate);
        inputDeviceListener.StartListening(generalSettings.InputDeviceIndex, generalSettings.InputOutputChannelOffsets);

        yield return new WaitForSecondsRealtime(generalSettings.TransientTimeInMs.ConvertToNormal(fromMetric: Metric.Milli));
        
        ComplexFloat calibratedImpedance = ComplexFloat.Zero;
        float elapsedRetryTimeoutInSec = 0f;
        
        for (int i = 0; i < generalSettings.AveragingIterations;)
        {
            if (inputDeviceListener.TryGetAndReleaseFilledSamplesByIntervals(
                generalSettings.CalibrationFrequency,
                generalSettings.SignalIntervalsCount,
                out ReadOnlySpan<float> inputDataSamples,
                out ReadOnlySpan<float> inputShiftDataSamples,
                out ReadOnlySpan<float> outputDataSamples
            ) == false)
            {
                yield return null;
                continue;
            }

            ComplexFloat computedImpedance = ZRLCHelper.ComputeImpedance(
                inputDataSamples,
                outputDataSamples,
                generalSettings.EquivalenceResistance,
                generalSettings.CalibrationFrequency,
                generalSettings.SamplingRate
            );

            if (float.IsNaN(computedImpedance.Magnitude) || float.IsNaN(computedImpedance.AngleInRad))
            {
                if (elapsedRetryTimeoutInSec > generalSettings.RetryTimeoutInSec)
                {
                    outputDeviceGenerator.StopGeneration();
                    inputDeviceListener.StopListening();
                    
                    OnCalibrationErrorOccurred.Invoke("Impedance is measured as NaN. Сheck your circuit and try again.");
                    yield break;
                }
                
                elapsedRetryTimeoutInSec += Time.deltaTime;
                yield return null;
                continue;
            }

            calibratedImpedance += computedImpedance;
            i++;
            
            elapsedRetryTimeoutInSec = 0f;
            yield return null;
        }
        
        outputDeviceGenerator.StopGeneration();
        inputDeviceListener.StopListening();
        
        calibratedImpedance /= generalSettings.AveragingIterations;
        getCalibratedImpedanceCallback.Invoke(calibratedImpedance);
    }
}
