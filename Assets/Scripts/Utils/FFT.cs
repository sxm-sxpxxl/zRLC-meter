using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Реализация прямого и обратного быстрых преобразований Фурье. 
/// </summary>
public static class FFT
{
    public static ComplexDouble[] ForwardTransform(ComplexDouble[] samples) => Transform(samples, isForward: true);

    public static ComplexDouble[] InverseTransform(ComplexDouble[] samples) => Transform(samples, isForward: false);
    
    private static ComplexDouble[] Transform(ComplexDouble[] samples, bool isForward)
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
        var r = new ComplexDouble(-1d, 0d);
        int l2 = 1;

        for (int l = 0; l < power; l++)
        {
            int l1 = l2;
            l2 <<= 1;

            var r2 = new ComplexDouble(1d, 0d);

            for (int n = 0; n < l1; n++)
            {
                for (int i = n; i < count; i += l2)
                {
                    int i1 = i + l1;
                    ComplexDouble temp = r2 * samples[i1];
                    samples[i1] = samples[i] - temp;
                    samples[i] += temp;
                }

                r2 *= r;
            }

            r.img = Mathf.Sqrt((float) (0.5d * (1d - r.real)));
            if (isForward)
            {
                r.img = -r.img;
            }

            r.real = Mathf.Sqrt((float) (0.5d * (1d + r.real)));
        }

        // Scaling for inverse transform
        if (isForward == false)
        {
            double scale = 1d / count;
            for (int i = 0; i < count; i++)
            {
                samples[i] *= scale;
            }
        }
        
        return samples;
    }
}
