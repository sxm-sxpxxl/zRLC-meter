using UnityEngine;

public enum SamplingRatePreset
{
    [InspectorName("8000 Hz")]
    SamplingRate1 = 8000,
    [InspectorName("16000 Hz")]
    SamplingRate2 = 16000,
    [InspectorName("32000 Hz")]
    SamplingRate3 = 32000,
    [InspectorName("44100 Hz")]
    SamplingRate4 = 44100,
    [InspectorName("48000 Hz")]
    Default = 48000,
    [InspectorName("96000 Hz")]
    SamplingRate6 = 96000
}

public enum ReferenceChannel
{
    Left = 0,
    Right = 1
}

public enum FrequencyIncrement
{
    [InspectorName("1 ⁒ 24 octave")]
    OneTwentyFourthOctave = 24,
    [InspectorName("1 ⁒ 48 octave")]
    OneFortyEighthOctave = 48
}

/// <summary>
/// Представляет основные настройки программы, доступные для чтения/записи различным компонентам.
/// </summary>
[CreateAssetMenu(fileName = "NewGeneralSettings", menuName = "ZRLCMeter/GeneralSettings", order = 0)]
public sealed class GeneralSettings : ScriptableObject
{
    [Header("Soundcard configuration")]
    [SerializeField] private int inputDeviceIndex = -1;
    [SerializeField] private int outputDeviceIndex = -1;
    [SerializeField, Range(0f, 1f)] private float outputDeviceVolume = 1f;
    
    [Header("Measurement configuration")]
    [SerializeField] private SamplingRatePreset samplingRate = SamplingRatePreset.Default;
    [SerializeField] private ReferenceChannel inputReference = ReferenceChannel.Left;
    [SerializeField, Min(0f)] private float equivalenceResistance = 100f;
    [SerializeField, Min(0f)] private float calibrationFrequency = 1000f;
    
    [Header("Frequency range configuration")]
    [SerializeField, Range(10f, 1000f)] private float transientTimeInMs = 100f;
    [SerializeField, Min(0f)] private float lowCutOffFrequency = 200f;
    [SerializeField, Min(0f)] private float highCutOffFrequency = 1000f;
    [SerializeField] private FrequencyIncrement frequencyIncrement = FrequencyIncrement.OneTwentyFourthOctave;
    
    [Header("Other")]
    [SerializeField, Range(0f, 5f)] private float retryTimeoutInSec = 1f;
    [SerializeField, Range(1, 100)] private int averagingIterations = 100;
    [SerializeField, Range(1, 10)] private int signalIntervalsCount = 3;

    public int InputDeviceIndex
    {
        get => inputDeviceIndex;
        set => inputDeviceIndex = value;
    }

    public int OutputDeviceIndex
    {
        get => outputDeviceIndex;
        set => outputDeviceIndex = value;
    }

    public float OutputDeviceVolume
    {
        get => outputDeviceVolume;
        set => outputDeviceVolume = Mathf.Clamp01(value);
    }

    public int SamplingRate
    {
        get => (int) samplingRate;
        set => samplingRate = (SamplingRatePreset) value;
    }
    
    public ReferenceChannel InputReferenceChannel
    {
        set => inputReference = value;
    }

    public (int, int) InputOutputChannelOffsets
    {
        get => inputReference == ReferenceChannel.Left ? (0, 1) : (1, 0);
    }

    public float EquivalenceResistance
    {
        get => equivalenceResistance;
        set => equivalenceResistance = Mathf.Max(value, 0f);
    }

    public float CalibrationFrequency
    {
        get => calibrationFrequency;
        set => calibrationFrequency = Mathf.Max(value, 0f);
    }

    public float TransientTimeInMs
    {
        get => transientTimeInMs;
        set => transientTimeInMs = Mathf.Clamp(value, 10f, 1000f);
    }

    public float LowCutOffFrequency
    {
        get => lowCutOffFrequency;
        set => lowCutOffFrequency = Mathf.Max(value, 0f);
    }

    public float HighCutOffFrequency
    {
        get => highCutOffFrequency;
        set => highCutOffFrequency = Mathf.Max(value, 0f);
    }
    
    public FrequencyIncrement FrequencyIncrement
    {
        get => frequencyIncrement;
        set => frequencyIncrement = value;
    }

    public float RetryTimeoutInSec
    {
        get => retryTimeoutInSec;
        set => retryTimeoutInSec = Mathf.Clamp(value, 0f, 5f);
    }

    public int AveragingIterations
    {
        get => averagingIterations;
        set => averagingIterations = Mathf.Clamp(value, 1, 100);
    }
    
    public int SignalIntervalsCount
    {
        get => signalIntervalsCount;
        set => signalIntervalsCount = Mathf.Clamp(value, 1, 10);
    }
}
