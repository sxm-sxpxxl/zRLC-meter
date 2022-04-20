using System;
using UnityEngine;
using UnityEngine.Assertions;

public enum SamplingRatePreset
{
    [InspectorName("8000 Hz")]
    Telephone = 8000,
    [InspectorName("44100 Hz")]
    AudioCD = 44100
}

public enum Channels
{
    One = 1,
    Two = 2
}

[RequireComponent(typeof(AudioSource))]
public sealed class WaveformGenerator : MonoBehaviour
{
    public event Action<float[]> OnSamplesChunkReady = delegate { };
    
    [Header("Audio Generation Settings")]
    [SerializeField] private SamplingRatePreset samplingRate = SamplingRatePreset.AudioCD;
    [SerializeField] private Channels channels = Channels.One;
    [SerializeField] private bool isLoop = false;
    [SerializeField, Range(0.1f, 1f)] private float clipLength = 1f;
    
    private AudioSource _audioSource;
    private int _currentSamplePosition, _samplingRate;

    [field: Header("Debug Info")]
    [field: SerializeField] public float WaveFrequency { get; private set; }
    [field: SerializeField] public bool IsGenerating { get; private set; }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void StartGeneration(float waveFrequency, SamplingRatePreset samplingRate)
    {
        Assert.IsTrue(waveFrequency > 0f);
        
        if (IsGenerating)
        {
            return;
        }

        WaveFrequency = waveFrequency;
        _samplingRate = (int) samplingRate;
        int samplesLength = Mathf.CeilToInt(_samplingRate * clipLength);
        
        _audioSource.clip = AudioClip.Create("Waveform", samplesLength, (int) channels, _samplingRate, true, OnPCMRead, OnPCMSetPosition);
        _audioSource.loop = isLoop;
        _audioSource.Play();
        
        IsGenerating = true;
    }

    public void StopGeneration()
    {
        if (IsGenerating == false)
        {
            return;
        }

        _audioSource.Stop();
        _audioSource.clip = null;
        _audioSource.loop = false;
        
        IsGenerating = false;
    }

    private void OnAudioFilterRead(float[] samplesChunk, int channels)
    {
        OnSamplesChunkReady.Invoke(samplesChunk);
    }

    private void OnPCMRead(float[] samplesChunk)
    {
        float normalizedWaveFactor = 2f * Mathf.PI * WaveFrequency;
        
        for (int i = 0; i < samplesChunk.Length; i++)
        {
            samplesChunk[i] = Mathf.Sin(normalizedWaveFactor * _currentSamplePosition / _samplingRate);
            _currentSamplePosition++;
        }
    }

    private void OnPCMSetPosition(int newPositionInSamples)
    {
        _currentSamplePosition = newPositionInSamples;
    }
}
