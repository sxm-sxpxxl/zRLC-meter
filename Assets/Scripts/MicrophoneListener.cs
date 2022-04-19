using System;
using UnityEngine;

public sealed class MicrophoneListener : MonoBehaviour
{
    public event Action<float[]> OnSamplesChunkReady = delegate { };

    [Header("Settings")]
    [SerializeField] private SamplingRateStandard samplingRate = SamplingRateStandard.AudioCD;
    [SerializeField, Range(0f, 1f)] private float latencyInSec = 0f;

    private int SamplingRate => (int) samplingRate;
    
    private void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning($"Recording devices weren't found!");
            return;
        }

        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            Debug.Log($"{i}: {Microphone.devices[i]}");
        }
        
        AudioClip recordingClip = Microphone.Start(Microphone.devices[0], true, 5, SamplingRate);
        GetComponent<AudioSource>().clip = recordingClip;
        GetComponent<AudioSource>().loop = true;
        
        while ((Microphone.GetPosition(null) > SamplingRate * latencyInSec) == false) {}
        GetComponent<AudioSource>().Play();
    }

    private void OnAudioFilterRead(float[] samplesChunk, int channels)
    {
        OnSamplesChunkReady.Invoke(samplesChunk);
    }
}
