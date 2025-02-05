using System;
public static class ActivationFunctions
{
    public static readonly Func<double, double> Sigmoid = x => 1.0 / (1.0 + Math.Exp(-x));
    public static readonly Func<double, double> Tanh = Math.Tanh;
    public static readonly Func<double, double> ReLU = x => x > 0 ? x : 0;
    public static readonly Func<double, double> LeakyReLU = x => x > 0 ? x : 0.01 * x;
    public static readonly Func<double, double> Linear = x => x;
}