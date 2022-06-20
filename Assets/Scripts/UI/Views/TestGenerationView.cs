using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TestGenerationView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;
    
    [Header("Test generation")]
    [SerializeField] private Button testGenerateButton;
    [SerializeField] private InputFieldController leftChannelRmsInputField;
    [SerializeField] private InputFieldController rightChannelRmsInputField;
    [SerializeField] private InputFieldController phaseShiftInputField;

    private InputDeviceListener _inputDeviceListener;
    private OutputDeviceGenerator _outputDeviceGenerator;

    private void Start()
    {
        testGenerateButton.onClick.AddListener(OnTestGenerationButtonClick);

        _inputDeviceListener = InputDeviceListener.Instance;
        _outputDeviceGenerator = OutputDeviceGenerator.Instance;

        Assert.IsNotNull(_inputDeviceListener);
        Assert.IsNotNull(_outputDeviceGenerator);
    }

    private void OnDestroy()
    {
        testGenerateButton.onClick.RemoveListener(OnTestGenerationButtonClick);
    }

    private void OnTestGenerationButtonClick()
    {
        testGenerateButton.interactable = false;
        
        StartCoroutine(TestGenerationCoroutine((leftChannelRms, rightChannelRms, phaseShiftInRad) =>
        {
            Debug.Log($"><color=yellow>Left channel RMS: </color> {leftChannelRms} V  " +
                      $"<color=yellow>Right channel RMS: </color> {rightChannelRms} V  " +
                      $"<color=yellow>Left/Right RMS ratio: </color> {leftChannelRms / rightChannelRms}  " +
                      $"<color=yellow>Phase shift (right-left): </color> {phaseShiftInRad * Mathf.Rad2Deg} °");
            
            leftChannelRmsInputField.SetValue(leftChannelRms);
            rightChannelRmsInputField.SetValue(rightChannelRms);
            phaseShiftInputField.SetValue(phaseShiftInRad * Mathf.Rad2Deg);
            
            testGenerateButton.interactable = true;
        }));
    }
    
    private IEnumerator TestGenerationCoroutine(Action<float, float, float> getResultCallback)
    {
        _outputDeviceGenerator.StartGeneration(generalSettings.OutputDeviceIndex, generalSettings.CalibrationFrequency, generalSettings.SamplingRate);
        _inputDeviceListener.StartListening(generalSettings.InputDeviceIndex, generalSettings.InputOutputChannelOffsets);

        yield return new WaitForSecondsRealtime(generalSettings.TransientTimeInMs.ConvertToNormal(fromMetric: Metric.Milli));
        
        float leftChannelRms = 0f, rightChannelRms = 0f, phaseShiftInRad = 0f;

        for (int i = 0; i < generalSettings.AveragingIterations;)
        {
            if (_inputDeviceListener.TryGetAndReleaseFilledSamplesByIntervals(
                generalSettings.CalibrationFrequency,
                generalSettings.SignalIntervalsCount,
                out ReadOnlySpan<float> leftChannelSamples,
                out ReadOnlySpan<float> rightChannelSamples
            ) == false)
            {
                yield return null;
                continue;
            }

            var leftChannelPeak = leftChannelSamples.ComplexPeak(generalSettings.CalibrationFrequency, generalSettings.SamplingRate);
            var rightChannelPeak = rightChannelSamples.ComplexPeak(generalSettings.CalibrationFrequency, generalSettings.SamplingRate);

            phaseShiftInRad += (rightChannelPeak / leftChannelPeak).AngleInRad;
            leftChannelRms += leftChannelSamples.Rms();
            rightChannelRms += rightChannelSamples.Rms();
            i++;

            yield return null;
        }
        
        _outputDeviceGenerator.StopGeneration();
        _inputDeviceListener.StopListening();

        phaseShiftInRad /= generalSettings.AveragingIterations;
        leftChannelRms /= generalSettings.AveragingIterations;
        rightChannelRms /= generalSettings.AveragingIterations;

        getResultCallback.Invoke(leftChannelRms, rightChannelRms, phaseShiftInRad);
    }
}
