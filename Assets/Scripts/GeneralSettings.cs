using UnityEngine;

public enum SamplingRatePreset
{
    [InspectorName("8000 Hz")]
    Telephone = 8000,
    [InspectorName("44100 Hz")]
    AudioCD = 44100
}

public enum ReferencePoint
{
    Left = 0,
    Right = 1
}

/// <summary>
/// Представляет основные настройки программы, доступные для чтения/записи различным компонентам.
/// </summary>
[CreateAssetMenu(fileName = "NewGeneralSettings", menuName = "ZRLCMeter/GeneralSettings", order = 0)]
public sealed class GeneralSettings : ScriptableObject
{
    [Header("I/O settings")]
    [SerializeField] private int inputDeviceIndex = -1;
    [SerializeField] private int outputDeviceIndex = -1;
    [SerializeField, Range(0f, 1f)] private float outputDeviceVolume = 1f;

    [Header("Measurement settings")]
    [SerializeField] private SamplingRatePreset sampleRate = SamplingRatePreset.AudioCD;
    [SerializeField] private ReferencePoint inputChannelReferencePoint = ReferencePoint.Left;
    [SerializeField, Min(0f)] private float equivalenceResistance = 100f;
    [SerializeField, Range(10f, 1000f)] private float transientTimeInMs = 100f;

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

    public int SampleRate
    {
        get => (int) sampleRate;
        set => sampleRate = (SamplingRatePreset) value;
    }
    
    public ReferencePoint InputChannelReferencePoint
    {
        get => inputChannelReferencePoint;
        set => inputChannelReferencePoint = value;
    }

    public (int, int) InputOutputChannelOffsets
    {
        get => inputChannelReferencePoint == ReferencePoint.Left ? (0, 1) : (1, 0);
    }

    public float EquivalenceResistance
    {
        get => equivalenceResistance;
        set => equivalenceResistance = Mathf.Max(value, 0f);
    }

    public float TransientTimeInMs
    {
        get => transientTimeInMs;
        set => transientTimeInMs = Mathf.Clamp(value, 10f, 1000f);
    }
}
