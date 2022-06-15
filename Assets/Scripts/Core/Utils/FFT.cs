using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Реализация прямого и обратного быстрых преобразований Фурье. 
/// </summary>
public static class FFT
{
    public static ComplexFloat[] ForwardTransform(ComplexFloat[] samples) => Transform(samples, isForward: true);

    public static ComplexFloat[] InverseTransform(ComplexFloat[] samples) => Transform(samples, isForward: false);
    
    private static ComplexFloat[] Transform(ComplexFloat[] samples, bool isForward)
    {
        Assert.IsTrue(samples.Length % 2 == 0, "Samples length need to be a power of two.");

        int power = (int) Mathf.Log(samples.Length, 2);
        int count = samples.Length;
        
        int halfCount = count >> 1;
        int j = 0;
        
        // Bit reversal
        for (int i = 0; i < count - 1; i++)
        {
            if (i < j)
            {
                (samples[i], samples[j]) = (samples[j], samples[i]);
            }

            int k = halfCount;
            while (k <= j)
            {
                j -= k;
                k >>= 1;
            }

            j += k;
        }

        // Compute the FFT
        var r = new ComplexFloat(-1f, 0f);
        int l2 = 1;

        for (int l = 0; l < power; l++)
        {
            int l1 = l2;
            l2 <<= 1;

            var r2 = new ComplexFloat(1f, 0f);

            for (int n = 0; n < l1; n++)
            {
                for (int i = n; i < count; i += l2)
                {
                    int i1 = i + l1;
                    ComplexFloat temp = r2 * samples[i1];
                    samples[i1] = samples[i] - temp;
                    samples[i] += temp;
                }

                r2 *= r;
            }

            r.imag = Mathf.Sqrt(0.5f * (1f - r.real));
            if (isForward)
            {
                r.imag = -r.imag;
            }

            r.real = Mathf.Sqrt(0.5f * (1f + r.real));
        }

        // Scaling for inverse transform
        if (isForward == false)
        {
            float scale = 1f / count;
            for (int i = 0; i < count; i++)
            {
                samples[i] *= scale;
            }
        }
        
        return samples;
    }
}
