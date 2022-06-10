using System;
using UnityEngine;

/// <summary>
/// Структура для хранения комплексных чисел вида Re + jIm, где Re - действительная часть, Im - мнимая.
/// </summary>
public struct ComplexFloat
{
    public float real;
    public float imag;

    public static ComplexFloat Zero => new ComplexFloat(0f, 0f);
    
    public float Magnitude => Mathf.Sqrt(real * real + imag * imag);
    public float AngleInRad => Mathf.Atan2(imag, real);

    public ComplexFloat AsReal => new ComplexFloat(real, 0f);
    public ComplexFloat AsImag => new ComplexFloat(0f, imag);

    public ComplexFloat(float real, float imag)
    {
        this.real = real;
        this.imag = imag;
    }

    public static ComplexFloat FromAngle(float angleInRad, float magnitude)
    {
        return new ComplexFloat(Mathf.Cos(angleInRad) * magnitude, Mathf.Sin(angleInRad) * magnitude);
    }
    
    public static ComplexFloat[] FloatToComplex(ReadOnlySpan<float> data, int length)
    {
        var complexData = new ComplexFloat[length];
        
        for (int i = 0; i < data.Length; i++)
        {
            complexData[i] = new ComplexFloat(data[i], 0f);
        }

        for (int i = data.Length; i < length; i++)
        {
            complexData[i] = ComplexFloat.Zero;
        }

        return complexData;
    }

    public static ComplexFloat operator +(ComplexFloat a, ComplexFloat b) =>
        new ComplexFloat(a.real + b.real, a.imag + b.imag);
    
    public static ComplexFloat operator +(ComplexFloat a, float b) => new ComplexFloat(a.real + b, a.imag);
    
    public static ComplexFloat operator +(float b, ComplexFloat a) => new ComplexFloat(a.real + b, a.imag);

    public static ComplexFloat operator -(ComplexFloat a) => new ComplexFloat(-a.real, -a.imag);

    public static ComplexFloat operator -(ComplexFloat a, ComplexFloat b) =>
        new ComplexFloat(a.real - b.real, a.imag - b.imag);

    public static ComplexFloat operator -(ComplexFloat a, float b) => new ComplexFloat(a.real - b, a.imag);

    public static ComplexFloat operator -(float b, ComplexFloat a) => new ComplexFloat(b - a.real, -a.imag);

    public static ComplexFloat operator *(ComplexFloat a, ComplexFloat b) =>
        new ComplexFloat(a.real * b.real - a.imag * b.imag, a.real * b.imag + a.imag * b.real);

    public static ComplexFloat operator *(ComplexFloat a, float b) => new ComplexFloat(a.real * b, a.imag * b);
    
    public static ComplexFloat operator *(float b, ComplexFloat a) => new ComplexFloat(a.real * b, a.imag * b);

    public static ComplexFloat operator /(ComplexFloat a, ComplexFloat b)
    {
        float d = 1f / (b.real * b.real + b.imag * b.imag);
        return new ComplexFloat((a.real * b.real + a.imag * b.imag) * d, (-a.real * b.imag + a.imag * b.real) * d);
    }

    public static ComplexFloat operator /(ComplexFloat a, float b) => new ComplexFloat(a.real / b, a.imag / b);

    public static ComplexFloat operator /(float a, ComplexFloat b)
    {
        float d = 1f / (b.real * b.real + b.imag * b.imag);
        return new ComplexFloat(a * b.real * d, -a * b.imag);
    }
}
