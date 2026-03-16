namespace Pricer.Numerics;

public static class CorrelatedBrownianGenerator
{
    private static readonly Random random = new();

    public static (double[] W1, double[] W3) Generate(
        int steps,
        double rho,
        double alpha)
    {
        // Generate two correlated Brownian motions W1 and W3 with correlation rho using W3 = rho*W1 + sqrt(1 - rho^2) * W2 (where W2 is an independent Brownian motion).
        double[] W1 = new double[steps];
        double[] W3 = new double[steps];

        double dt = 1.0 / steps;

        for (int i = 0; i < steps; i++)
        {
            double z1 = NextGaussian();
            double z2 = NextGaussian();

            double w1 = Math.Pow(dt, alpha / 2.0) * z1;
            double w2 = Math.Pow(dt, alpha / 2.0) * z2;

            double w3 = rho * w1 + Math.Sqrt(1 - rho * rho) * w2;

            W1[i] = w1;
            W3[i] = w3;
        }

        return (W1, W3);
    }

    private static double NextGaussian()
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();

        return Math.Sqrt(-2.0 * Math.Log(u1)) *
               Math.Cos(2.0 * Math.PI * u2);
    }
}