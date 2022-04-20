using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class MicrophoneListener : MonoBehaviour
{
    public event Action<float[]> OnSamplesChunkReady = delegate { };

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] private float latencyInSec = 0f;

    private AudioSource _audioSource;
    private string _listeningDeviceName;

    [field: Header("Debug Info"), SerializeField]
    public bool IsListening { get; private set; } = false;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartListening(SamplingRatePreset.AudioCD);
    }

    public void StartListening(SamplingRatePreset samplingRate)
    {
        if (IsListening)
        {
            return;
        }
        
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning($"Recording devices weren't found!");
            return;
        }

        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            Debug.Log($"{i}: {Microphone.devices[i]}");
        }

        _listeningDeviceName = Microphone.devices[0];
        
        _audioSource.clip = Microphone.Start(_listeningDeviceName, true, 5, (int) samplingRate);
        _audioSource.loop = true;
        
        while ((Microphone.GetPosition(null) > (int) samplingRate * latencyInSec) == false) {}
        _audioSource.Play();
        
        IsListening = true;
    }

    public void StopListening()
    {
        if (IsListening == false)
        {
            return;
        }
        
        Microphone.End(_listeningDeviceName);
        
        _audioSource.Stop();
        _audioSource.clip = null;
        _audioSource.loop = false;

        IsListening = false;
    }
    
    private void OnAudioFilterRead(float[] samplesChunk, int channels)
    {
        OnSamplesChunkReady.Invoke(samplesChunk);
    }
}
