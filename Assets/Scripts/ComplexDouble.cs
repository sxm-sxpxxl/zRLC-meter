using System;
using System.Diagnostics;
using UnityEngine;

[DebuggerDisplay("{real} + {img}i")]
public struct ComplexDouble
{
    public double real;
    public double img;

    public double Magnitude => Math.Sqrt(SqrMagnitude);

    public double SqrMagnitude => real * real + img * img;

    public double AngleInRad => Math.Atan2((float) img, (float) real);

    public double AngleInDeg => AngleInRad * Mathf.Rad2Deg;
    
    public ComplexDouble(double real, double img)
    {
        this.real = real;
        this.img = img;
    }

    public static ComplexDouble[] DoubleToComplex(double[] data)
    {
        var complexData = new ComplexDouble[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            complexData[i] = new ComplexDouble(data[i], 0f);
        }

        return complexData;
    }

    public static ComplexDouble[] FloatToComplex(float[] data)
    {
        var complexData = new ComplexDouble[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            complexData[i] = new ComplexDouble(data[i], 0f);
        }

        return complexData;
    }

    public static ComplexDouble operator +(ComplexDouble a, ComplexDouble b) => new(a.real + b.real, a.img + b.img);
    
    public static ComplexDouble operator +(ComplexDouble a, double b) => new(a.real + b, a.img);
    
    public static ComplexDouble operator +(double b, ComplexDouble a) => new(a.real + b, a.img);
    
    public static ComplexDouble operator -(ComplexDouble a) => new(-a.real, -a.img);

    public static ComplexDouble operator -(ComplexDouble a, ComplexDouble b) => new(a.real - b.real, a.img - b.img);
    
    public static ComplexDouble operator -(ComplexDouble a, double b) => new(a.real - b, a.img);
    
    public static ComplexDouble operator -(double b, ComplexDouble a) => new(a.real - b, a.img);
    
    public static ComplexDouble operator *(ComplexDouble a, ComplexDouble b) => new(a.real * b.real - a.img * b.img, a.real * b.img + a.img * b.real);
    
    public static ComplexDouble operator *(ComplexDouble a, double b) => new(a.real * b, a.img * b);
    
    public static ComplexDouble operator *(double b, ComplexDouble a) => new(a.real * b, a.img * b);

    public static ComplexDouble operator /(ComplexDouble a, ComplexDouble b)
    {
        double d = 1f / (b.real * b.real + b.img * b.img);
        return new(d * (a.real * b.real + a.img * b.img), d * (-a.real * b.img + a.img * b.real));
    }

    public static ComplexDouble operator /(ComplexDouble a, double b) => new(a.real / b, a.img / b);

    public static ComplexDouble operator /(double a, ComplexDouble b)
    {
        double d = 1f / (b.real * b.real + b.img * b.img);
        return new(a * b.real * d, -a * b.img);
    }
}