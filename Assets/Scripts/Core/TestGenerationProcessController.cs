using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public sealed class TestGenerationProcessController : MonoBehaviour
{
    public event Action OnTestGenerationStarted = delegate { };
    public event Action<float, float, float> OnTestGenerationFinished = delegate { };
    public event Action<float> OnTestGenerationProgressUpdated = delegate { };

    [Header("Dependencies")]
    [SerializeField] private GeneralSettings generalSettings;
    
    private InputDeviceListener _inputDeviceListener;
    private OutputDeviceGenerator _outputDeviceGenerator;

    private void Start()
    {
        _inputDeviceListener = InputDeviceListener.Instance;
        _outputDeviceGenerator = OutputDeviceGenerator.Instance;

        Assert.IsNotNull(_inputDeviceListener);
        Assert.IsNotNull(_outputDeviceGenerator);
    }

    public void TestGenerate()
    {
        StartCoroutine(TestGenerationCoroutine((leftChannelRms, rightChannelRms, phaseShiftInRad) =>
        {
            Debug.Log($"><color=yellow>Left channel RMS: </color> {leftChannelRms} V  " +
                      $"<color=yellow>Right channel RMS: </color> {rightChannelRms} V  " +
                      $"<color=yellow>Left/Right RMS ratio: </color> {leftChannelRms / rightChannelRms}  " +
                      $"<color=yellow>Phase shift (right-left): </color> {phaseShiftInRad * Mathf.Rad2Deg} °");

            OnTestGenerationFinished.Invoke(leftChannelRms, rightChannelRms, phaseShiftInRad);
        }));

        OnTestGenerationStarted.Invoke();
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
            
            OnTestGenerationProgressUpdated.Invoke((float) i / generalSettings.AveragingIterations);

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
