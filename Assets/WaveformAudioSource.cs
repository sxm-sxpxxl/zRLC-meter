using UnityEngine;

public enum SamplingRateStandard
{
    [InspectorName("Telephone (8000 Hz)")]
    Telephone = 8000,
    [InspectorName("Audio CD (44100 Hz)")]
    AudioCD = 44100,
    [InspectorName("DVD-Audio (96000 Hz)")]
    DVDAudio = 96000
}

public enum Channels
{
    One = 1,
    Two = 2
}

[RequireComponent(typeof(AudioSource))]
public sealed class WaveformAudioSource : MonoBehaviour
{
    [Header("Audio Generation Settings")]
    [SerializeField] private SamplingRateStandard samplingRate = SamplingRateStandard.AudioCD;
    [SerializeField] private Channels channels = Channels.One;
    [SerializeField] private bool isLoop = false;
    
    [Header("Waveform Settings")]
    [SerializeField, Range(20, 20000)] private int waveFrequency = 440;

    private AudioSource _audioSource;
    private int _currentSamplePosition;

    private int SamplingRate => (int) samplingRate;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        AudioClip generatedWaveformClip = AudioClip.Create("Waveform", SamplingRate, (int) channels, SamplingRate, true, OnPCMRead, OnPCMSetPosition);
        
        _audioSource.clip = generatedWaveformClip;
        _audioSource.loop = isLoop;
        
        _audioSource.Play();
    }

    private void OnPCMRead(float[] samplesChunk)
    {
        for (int i = 0; i < samplesChunk.Length; i++)
        {
            samplesChunk[i] = Mathf.Sin(2f * Mathf.PI * waveFrequency * _currentSamplePosition / SamplingRate);
            _currentSamplePosition++;
        }
    }

    private void OnPCMSetPosition(int newPositionInSamples)
    {
        _currentSamplePosition = newPositionInSamples;
    }
}
