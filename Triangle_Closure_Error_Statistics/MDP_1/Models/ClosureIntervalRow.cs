namespace MDP_1.Models;

public class ClosureIntervalRow
{
    public required string IntervalLabel { get; init; }

    public int NegativeCount { get; init; }

    public double NegativeFrequency { get; init; }

    public double NegativeDensity { get; init; }

    public int PositiveCount { get; init; }

    public double PositiveFrequency { get; init; }

    public double PositiveDensity { get; init; }
}

