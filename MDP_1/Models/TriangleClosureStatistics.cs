using System.Globalization;

namespace MDP_1.Models;

public class TriangleClosureStatistics
{
    public double BinWidth { get; }

    public double MaxAbsoluteValue { get; }

    public IReadOnlyList<ClosureIntervalRow> Rows { get; }

    public int NegativeTotalCount { get; }

    public int PositiveTotalCount { get; }

    private TriangleClosureStatistics(
        double binWidth,
        double maxAbsoluteValue,
        IReadOnlyList<ClosureIntervalRow> rows,
        int negativeTotalCount,
        int positiveTotalCount)
    {
        BinWidth = binWidth;
        MaxAbsoluteValue = maxAbsoluteValue;
        Rows = rows;
        NegativeTotalCount = negativeTotalCount;
        PositiveTotalCount = positiveTotalCount;
    }

    public int SumNegativeCounts() => Rows.Sum(r => r.NegativeCount);

    public int SumPositiveCounts() => Rows.Sum(r => r.PositiveCount);

    public double SumNegativeFrequencies() => Rows.Sum(r => r.NegativeFrequency);

    public double SumPositiveFrequencies() => Rows.Sum(r => r.PositiveFrequency);

    public static TriangleClosureStatistics CreateFromValues(
        IEnumerable<double> values,
        double binWidth = 0.2,
        double maxAbsoluteValue = 2.6)
    {
        if (binWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(binWidth), "binWidth must be greater than 0.");
        }

        if (maxAbsoluteValue <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAbsoluteValue), "maxAbsoluteValue must be greater than 0.");
        }

        var binCount = (int)Math.Ceiling(maxAbsoluteValue / binWidth);
        var negativeCounts = new int[binCount + 1];
        var positiveCounts = new int[binCount + 1];

        foreach (var value in values)
        {
            var abs = Math.Abs(value);
            var index = abs >= maxAbsoluteValue
                ? binCount
                : Math.Min((int)Math.Floor(abs / binWidth), binCount - 1);

            if (value < 0)
            {
                negativeCounts[index]++;
            }
            else
            {
                // 将 0 归入正侧，确保每条观测都被计入统计。
                positiveCounts[index]++;
            }
        }

        var negativeTotal = negativeCounts.Sum();
        var positiveTotal = positiveCounts.Sum();
        var rows = new List<ClosureIntervalRow>(binCount + 1);

        for (var i = 0; i < binCount; i++)
        {
            var lower = i * binWidth;
            var upper = (i + 1) * binWidth;
            rows.Add(new ClosureIntervalRow
            {
                IntervalLabel = string.Create(
                    CultureInfo.InvariantCulture,
                    $"{lower:0.00}~{upper:0.00}"),
                NegativeCount = negativeCounts[i],
                NegativeFrequency = negativeTotal == 0 ? 0 : (double)negativeCounts[i] / negativeTotal,
                NegativeDensity = negativeTotal == 0 ? 0 : ((double)negativeCounts[i] / negativeTotal) / binWidth,
                PositiveCount = positiveCounts[i],
                PositiveFrequency = positiveTotal == 0 ? 0 : (double)positiveCounts[i] / positiveTotal,
                PositiveDensity = positiveTotal == 0 ? 0 : ((double)positiveCounts[i] / positiveTotal) / binWidth,
            });
        }

        rows.Add(new ClosureIntervalRow
        {
            IntervalLabel = string.Create(CultureInfo.InvariantCulture, $">{maxAbsoluteValue:0.00}"),
            NegativeCount = negativeCounts[binCount],
            NegativeFrequency = negativeTotal == 0 ? 0 : (double)negativeCounts[binCount] / negativeTotal,
            NegativeDensity = 0,
            PositiveCount = positiveCounts[binCount],
            PositiveFrequency = positiveTotal == 0 ? 0 : (double)positiveCounts[binCount] / positiveTotal,
            PositiveDensity = 0,
        });

        return new TriangleClosureStatistics(
            binWidth,
            maxAbsoluteValue,
            rows,
            negativeTotal,
            positiveTotal);
    }
}
