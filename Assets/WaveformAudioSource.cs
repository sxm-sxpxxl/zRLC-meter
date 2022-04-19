using System;
using UnityEngine;

public enum SamplingRateStandard
{
    [InspectorName("Telephone (8000 Hz)")]
    Telephone = 8000,
    [InspectorName("Audio CD (44100 Hz)")]
    AudioCD = 44100
}

public enum Channels
{
    One = 1,
    Two = 2
}

[RequireComponent(typeof(AudioSource))]
public sealed class WaveformAudioSource : MonoBehaviour
{
    public event Action<float[], int> OnSamplesChunkReady = delegate { };
    
    [Header("Audio Generation Settings")]
    [SerializeField] private SamplingRateStandard samplingRate = SamplingRateStandard.AudioCD;
    [SerializeField] private Channels channels = Channels.One;
    [SerializeField] private bool isLoop = false;
    [SerializeField, Range(0.1f, 1f)] private float clipLength = 1f;
    
    [Header("Waveform Settings")]
    [SerializeField, Range(20, 20000)] private int waveFrequency = 440;

    private AudioSource _audioSource;
    private int _currentSamplePosition;
    private float[] _samples;
    
    private int SamplingRate => (int) samplingRate;
    private int SamplesLength => Mathf.CeilToInt(SamplingRate * clipLength);

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _samples = new float[SamplesLength];
        AudioClip generatedWaveformClip = AudioClip.Create("Waveform", SamplesLength, (int) channels, SamplingRate, true, OnPCMRead, OnPCMSetPosition);
        
        _audioSource.clip = generatedWaveformClip;
        _audioSource.loop = isLoop;
        
        _audioSource.Play();
    }

    private void OnPCMRead(float[] samplesChunk)
    {
        for (int i = 0; i < samplesChunk.Length; i++)
        {
            samplesChunk[i] = Mathf.Sin(2f * Mathf.PI * waveFrequency * _currentSamplePosition / SamplingRate);

            if (_currentSamplePosition < _samples.Length)
            {
                _samples[_currentSamplePosition] = samplesChunk[i];
            }
            
            _currentSamplePosition++;
        }
    }

    private void OnPCMSetPosition(int newPositionInSamples)
    {
        OnSamplesChunkReady(_samples, 0);
        _currentSamplePosition = newPositionInSamples;
    }
}
