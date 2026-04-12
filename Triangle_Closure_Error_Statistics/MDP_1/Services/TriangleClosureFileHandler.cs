using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using MDP_1.Models;

namespace MDP_1.Services;

public class TriangleClosureFileHandler
{
    public List<double> TriangleClosureDifferences { get; } = new();

    public void ReadTriangleClosureDifferences(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Input file does not exist.", filePath);
        }

        TriangleClosureDifferences.Clear();

        var lines = File.ReadAllLines(filePath);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            {
                continue;
            }

            var tokens = line.Split(new[] { ',', ';', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                continue;
            }

            // 分组模式：lower,upper,negativeCount,positiveCount。
            if (tokens.Length >= 4
                && TryParseDouble(tokens[0], out var lower)
                && TryParseDouble(tokens[1], out var upper)
                && int.TryParse(tokens[2], out var negativeCount)
                && int.TryParse(tokens[3], out var positiveCount))
            {
                var middle = (lower + upper) / 2.0;
                for (var i = 0; i < Math.Max(0, negativeCount); i++)
                {
                    TriangleClosureDifferences.Add(-Math.Abs(middle));
                }

                for (var i = 0; i < Math.Max(0, positiveCount); i++)
                {
                    TriangleClosureDifferences.Add(Math.Abs(middle));
                }

                continue;
            }

            // 原始值模式：每个可解析数字都视为一个闭合差。
            var parsedAny = false;
            foreach (var token in tokens)
            {
                if (TryParseDouble(token, out var value))
                {
                    TriangleClosureDifferences.Add(value);
                    parsedAny = true;
                }
            }

            // 忽略不包含数字的行（如表头文本）。
            if (!parsedAny)
            {
                continue;
            }
        }
    }

    public void WriteStatisticsResult(string filePath, TriangleClosureStatistics statistics)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension == ".csv")
        {
            WriteCsv(filePath, statistics);
            return;
        }

        WriteText(filePath, statistics);
    }

    private static void WriteCsv(string filePath, TriangleClosureStatistics statistics)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.WriteLine("Interval,NegativeCount,NegativeFrequency,NegativeDensity,PositiveCount,PositiveFrequency,PositiveDensity");

        foreach (var row in statistics.Rows)
        {
            writer.WriteLine(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"{row.IntervalLabel},{row.NegativeCount},{row.NegativeFrequency:F6},{row.NegativeDensity:F6},{row.PositiveCount},{row.PositiveFrequency:F6},{row.PositiveDensity:F6}"));
        }

        writer.WriteLine();
        writer.WriteLine($"NegativeTotal,{statistics.NegativeTotalCount}");
        writer.WriteLine($"PositiveTotal,{statistics.PositiveTotalCount}");
        writer.WriteLine(string.Create(CultureInfo.InvariantCulture, $"NegativeFrequencySum,{statistics.SumNegativeFrequencies():F6}"));
        writer.WriteLine(string.Create(CultureInfo.InvariantCulture, $"PositiveFrequencySum,{statistics.SumPositiveFrequencies():F6}"));
    }

    private static void WriteText(string filePath, TriangleClosureStatistics statistics)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.WriteLine("Triangle Closure Difference Statistics");
        writer.WriteLine($"Bin width d = {statistics.BinWidth:0.00}");
        writer.WriteLine($"Max absolute value = {statistics.MaxAbsoluteValue:0.00}");
        writer.WriteLine($"Negative total count = {statistics.NegativeTotalCount}");
        writer.WriteLine($"Positive total count = {statistics.PositiveTotalCount}");
        writer.WriteLine();
        writer.WriteLine("Interval            NegCount    NegFreq      NegDensity   PosCount    PosFreq      PosDensity");

        foreach (var row in statistics.Rows)
        {
            writer.WriteLine(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"{row.IntervalLabel,-18}{row.NegativeCount,10}{row.NegativeFrequency,12:F6}{row.NegativeDensity,13:F6}{row.PositiveCount,11}{row.PositiveFrequency,12:F6}{row.PositiveDensity,13:F6}"));
        }

        writer.WriteLine();
        writer.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Negative frequency sum: {statistics.SumNegativeFrequencies():F6}"));
        writer.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Positive frequency sum: {statistics.SumPositiveFrequencies():F6}"));
    }

    private static bool TryParseDouble(string token, out double value)
    {
        return double.TryParse(token, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value)
               || double.TryParse(token, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value);
    }
}
