using MathNet.Numerics.Distributions;


namespace Pricer.Numerics;

public enum OptionType
{
    Call,
    Put
}

public class BlackScholes
{
    private OptionType option;
    private double r; // Risk-free interest rate
    private double T; // Time to maturity
    private double sigma; // Volatility
    private double K; // Strike price
    private double S; // Underlying asset price
    private double q; // Dividend yield

    public BlackScholes(
        OptionType optionType,
        double riskFreeRate,
        double timeToMaturity,
        double volatility,
        double strike,
        double underlyingPrice,
        double dividendYield = 0.0)
    {
        option = optionType;
        r = riskFreeRate;
        T = timeToMaturity;
        sigma = volatility;
        K = strike;
        S = underlyingPrice;
        q = dividendYield;
    }

    // Cumulative normal distribution
    private static double N(double x)
        => Normal.CDF(0.0, 1.0, x);

    // Normal probability density function
    private static double n(double x)
        => Math.Exp(-0.5 * x * x) / Math.Sqrt(2.0 * Math.PI);

    // Constants for Black-Scholes
    private double d1 
        => (Math.Log(S / K) + (r - q + 0.5 * sigma * sigma) * T) / (sigma * Math.Sqrt(T));

    private double d2 
        => (Math.Log(S / K) - (r - q + 0.5 * sigma * sigma) * T) / (sigma * Math.Sqrt(T));


    // Black–Scholes price
    public double Price()
    {
        // Handle expiry case
        if (T <= 0.0)
        {
            return option switch
            {
                OptionType.Call => Math.Max(S - K, 0.0),
                OptionType.Put => Math.Max(K - S, 0.0),
                _ => throw new ArgumentException("Invalid option type")
            };
        }

        // Handle deterministic case (zero volatility)
        if (sigma <= 0.0)
        {
             return option switch
            {
                OptionType.Call => Math.Max(S * Math.Exp(- q * T) - Math.Exp(-r * T) * K, 0.0),
                OptionType.Put => Math.Max(Math.Exp(-r * T) * K - S * Math.Exp(- q * T), 0.0),
                _ => throw new ArgumentException("Invalid option type")
            };
        }

        return option switch
        {
            OptionType.Call => S * Math.Exp(-q * T) * N(d1) - K * Math.Exp(-r * T) * N(d2),

            OptionType.Put => K * Math.Exp(-r * T) * N(-d2) - S * Math.Exp(-q * T) * N(-d1),

            _ => throw new ArgumentException("Invalid option type")
        };
    }

    public double Vega()
    {
        if (T <= 0.0 || sigma <= 0.0)
            return 0.0;

        return S * Math.Exp(-q * T) * Math.Sqrt(T) * n(d1);
    }

}

// Test price compared with party at the mooonlight option calculator
// Test put call parity for consistency


public static class ImpliedVolatilityCalculator
{
    public static double Compute(
        OptionType optionType,
        double marketPrice,
        double interestRate,
        double timeToMaturity,
        double strike,
        double underlyingPrice,
        double initialGuess = 0.2,
        double tolerance = 1e-8,
        int maxIterations = 100)
    {
        if (marketPrice <= 0.0)
            throw new ArgumentException("Market price cannot be negative or zero.");

        if (timeToMaturity <= 0.0)
            throw new ArgumentException("Implied volatility undefined at maturity.");

        if (strike <= 0.0)
            throw new ArgumentException("Strike cannot be negative or zero.");

        if (underlyingPrice <= 0.0)
            throw new ArgumentException("Underlying price cannot be negative or zero.");

        if (initialGuess <= 0.0)
            initialGuess = 0.2;

        double sigma = initialGuess;

        for (int i = 0; i < maxIterations; i++)
        {
            var bs = new BlackScholes(optionType, interestRate, timeToMaturity, sigma, strike, underlyingPrice);

            double price = bs.Price();
            double vega = bs.Vega();

            double error = price - marketPrice;

            if (Math.Abs(error) < tolerance)
                return sigma;

            if (vega <= 1e-10)
                vega += 0.01; // Avoid division by zero

            sigma -= error / vega;

            // Prevent negative volatility
            if (sigma <= 0)
                sigma = 1e-10;
        }
         // Best estimate under maxIterations
        return sigma;
    }

    public static double BachelierImpliedVolATM(
        double optionPrice,
        double underlyingPrice,
        double timeToMaturity,
        double interestRate)
    {
        if (timeToMaturity <= 0.0)
            throw new ArgumentException("Implied normal volatility undefined at maturity.");

        if (optionPrice <= 0.0)
            throw new ArgumentException("Option price cannot be negative or zero.");

        double discountFactor = Math.Exp(-interestRate * timeToMaturity);

        // ATM Bachelier formula inversion
        return optionPrice * Math.Sqrt(2.0 * Math.PI) / (Math.Exp(-interestRate * timeToMaturity) * Math.Sqrt(timeToMaturity));
    }
}