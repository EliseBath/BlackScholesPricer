namespace Pricer.Numerics;

public class BinomialTreePricer
{
    private OptionType option;
    private double r;
    private double T;
    private double sigma;
    private double K;
    private double S;
    private int n;

    public BinomialTreePricer(
        OptionType optionType,
        double riskFreeRate,
        double timeToMaturity,
        double volatility,
        double strike,
        double underlyingPrice,
        int steps)
    {
        option = optionType;
        r = riskFreeRate;
        T = timeToMaturity;
        sigma = volatility;
        K = strike;
        S = underlyingPrice;
        n = steps;
    }

    public double Price()
    {
        double dt = T / n;

        double u = Math.Exp(sigma * Math.Sqrt(dt));

        double d = 1.0 / u;

        double p =
            (Math.Exp(r * dt) - d)
            / (u - d);

        double discount = Math.Exp(-r * dt);

        double[] optionValues = new double[n + 1];

        // TERMINAL PAYOFFS
        for (int i = 0; i <= n; i++)
        {
            double ST =
                S
                * Math.Pow(u, i)
                * Math.Pow(d, n - i);

            optionValues[i] = option switch
            {
                OptionType.Call => Math.Max(ST - K, 0.0),

                OptionType.Put => Math.Max(K - ST, 0.0),

                _ => 0.0
            };
        }

        // BACKWARD INDUCTION
        for (int step = n - 1; step >= 0; step--)
        {
            for (int i = 0; i <= step; i++)
            {
                optionValues[i] =
                    discount
                    * (
                        p * optionValues[i + 1]
                        + (1.0 - p) * optionValues[i]
                    );
            }
        }

        return optionValues[0];
    }
}