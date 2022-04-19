using UnityEngine;

public static class DFT
{
    public static ComplexDouble[] ForwardTransform(ComplexDouble[] samples) => Transform(samples, isForward: true);
    
    public static ComplexDouble[] InverseTransform(ComplexDouble[] samples) => Transform(samples, isForward: false);
    
    private static ComplexDouble[] Transform(ComplexDouble[] samples, bool isForward)
    {
        var tempSamples = new ComplexDouble[samples.Length];
        
        for (int i = 0; i < samples.Length; i++)
        {
            tempSamples[i].real = 0;
            tempSamples[i].img = 0;

            float dir = isForward ? 1f : -1f;
            float arg = (float) (-dir * 2.0 * Mathf.PI * i / samples.Length);
            
            for (int k = 0; k < samples.Length; k++)
            {
                float cosarg = Mathf.Cos(k * arg);
                float sinarg = Mathf.Sin(k * arg);
                
                tempSamples[i].real += samples[k].real * cosarg - samples[k].img * sinarg;
                tempSamples[i].img += samples[k].real * sinarg + samples[k].img * cosarg;
            }
        }

        if (isForward)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = tempSamples[i];
            }
        }
        else
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = tempSamples[i] / samples.Length;
            }
        }
        
        return samples;
    }
}
