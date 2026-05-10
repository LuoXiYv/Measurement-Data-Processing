using System.Globalization;
using System.IO;
using MDP_2.Models;

namespace MDP_2.Services;

public static class CsvDataLoader
{
    public static (List<LPointClass> Points, List<LineClass> Lines) Parse(string text)
    {
        var points = new List<LPointClass>();
        var lines = new List<LineClass>();

        var rows = text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .Where(r => !string.IsNullOrWhiteSpace(r) && !r.StartsWith("#"))
            .ToList();

        var section = string.Empty;
        foreach (var row in rows)
        {
            if (row.Equals("[Points]", StringComparison.OrdinalIgnoreCase))
            {
                section = "points";
                continue;
            }

            if (row.Equals("[Edges]", StringComparison.OrdinalIgnoreCase))
            {
                section = "edges";
                continue;
            }

            if (row.StartsWith("PID", StringComparison.OrdinalIgnoreCase) ||
                row.StartsWith("LID", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var tokens = row.Split(',', StringSplitOptions.TrimEntries);
            if (section == "points")
            {
                points.Add(ParsePoint(tokens));
            }
            else if (section == "edges")
            {
                lines.Add(ParseLine(tokens));
            }
        }

        if (points.Count == 0 || lines.Count == 0)
        {
            throw new InvalidDataException("CSV内容不完整，请检查[Points]和[Edges]两部分。");
        }

        return (points, lines);
    }

    public static string GetEmbeddedExampleCsv()
    {
        return """
               [Points]
               PID,H,IsControlP,IsCommonP
               PA,12.248,true,false
               1,,false,false
               2,,false,false
               3,,false,false
               4,,false,false
               PB,10.505,true,false

               [Edges]
               LID,SPID,EPID,ForwardDH,BackwardDH,Distance
               1,PA,1,3.248,3.240,4.0
               2,1,2,0.348,0.356,3.2
               3,2,3,1.444,1.437,2.0
               4,3,4,-3.360,-3.352,2.6
               5,4,PB,-3.699,-3.704,3.4
               """;
    }

    private static LPointClass ParsePoint(string[] tokens)
    {
        if (tokens.Length < 4)
        {
            throw new InvalidDataException($"点数据格式错误：{string.Join(',', tokens)}");
        }

        var pid = tokens[0];
        var h = ParseOptionalDouble(tokens[1]);
        var isControl = ParseBool(tokens[2]);
        var isCommon = ParseBool(tokens[3]);

        var point = new LPointClass
        {
            PID = pid,
            IsControlP = isControl,
            IsCommonP = isCommon,
            IsH0 = h.HasValue,
            H = h ?? 0.0
        };

        point.InitialH = h ?? 10000.0;
        point.AdjustedH = point.InitialH;
        return point;
    }

    private static LineClass ParseLine(string[] tokens)
    {
        if (tokens.Length < 6)
        {
            throw new InvalidDataException($"边数据格式错误：{string.Join(',', tokens)}");
        }

        return new LineClass
        {
            LID = tokens[0],
            SPID = tokens[1],
            EPID = tokens[2],
            ForwardDH = ParseRequiredDouble(tokens[3]),
            BackwardDH = ParseRequiredDouble(tokens[4]),
            Distance = ParseRequiredDouble(tokens[5])
        };
    }

    private static bool ParseBool(string value)
    {
        if (bool.TryParse(value, out var b))
        {
            return b;
        }

        return value.Trim() switch
        {
            "1" => true,
            "0" => false,
            "是" => true,
            _ => false
        };
    }

    private static double ParseRequiredDouble(string value)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
        {
            return d;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out d))
        {
            return d;
        }

        throw new InvalidDataException($"数字解析失败：{value}");
    }

    private static double? ParseOptionalDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return ParseRequiredDouble(value);
    }
}
