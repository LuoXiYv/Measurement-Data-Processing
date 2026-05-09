using System.Globalization;

namespace Matrix_mod;

public static class MatrixParser
{
    public static bool TryParse(string? text, out Matrix? matrix, out string? error)
    {
        matrix = null;
        error = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            error = "请输入矩阵内容。";
            return false;
        }

        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            error = "矩阵内容为空。";
            return false;
        }

        double[] values;
        var rowCount = lines.Length;
        var colCount = -1;
        values = Array.Empty<double>();

        for (var r = 0; r < lines.Length; r++)
        {
            var parts = lines[r].Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                error = $"第 {r + 1} 行没有有效数据。";
                return false;
            }

            if (colCount < 0)
            {
                colCount = parts.Length;
                values = new double[rowCount * colCount];
            }
            else if (parts.Length != colCount)
            {
                error = $"第 {r + 1} 行列数不一致。";
                return false;
            }

            for (var c = 0; c < parts.Length; c++)
            {
                if (!TryParseNumber(parts[c], out var number))
                {
                    error = $"第 {r + 1} 行第 {c + 1} 列无法解析：{parts[c]}";
                    return false;
                }

                values[(r * colCount) + c] = number;
            }
        }

        matrix = Matrix.CreateZero(rowCount, colCount);
        for (var r = 0; r < rowCount; r++)
        {
            for (var c = 0; c < colCount; c++)
            {
                matrix.SetValue(r, c, values[(r * colCount) + c]);
            }
        }

        return true;
    }

    private static bool TryParseNumber(string text, out double value)
    {
        return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value)
            || double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value);
    }
}
