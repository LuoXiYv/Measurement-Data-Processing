using System;
using System.Text;

namespace Matrix_mod;

public sealed class Matrix
{
    private double[] _values;
    private int _rows;
    private int _cols;

    public int Rows
    {
        get => _rows;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "行数必须大于 0。");
            }

            _rows = value;
            ResizeStorage();
        }
    }

    public int Cols
    {
        get => _cols;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "列数必须大于 0。");
            }

            _cols = value;
            ResizeStorage();
        }
    }

    // 索引器仅提供读取。
    public double this[int row, int col] => _values[(row * _cols) + col];

    // 根据行列随机创建矩阵，元素范围 1~100。
    public Matrix(int rows, int cols)
    {
        if (rows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "行数必须大于 0。");
        }

        if (cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cols), "列数必须大于 0。");
        }

        _rows = rows;
        _cols = cols;
        _values = new double[rows * cols];

        for (var i = 0; i < _values.Length; i++)
        {
            _values[i] = Random.Shared.Next(1, 101);
        }
    }

    private Matrix(int rows, int cols, double[] values)
    {
        _rows = rows;
        _cols = cols;
        _values = values;
    }

    public static Matrix CreateZero(int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "行列必须大于 0。");
        }

        return new Matrix(rows, cols, new double[rows * cols]);
    }

    public void SetValue(int row, int col, double value)
    {
        ValidateIndex(row, col);
        _values[(row * _cols) + col] = value;
    }

    public Matrix Inverse()
    {
        if (_rows != _cols)
        {
            throw new InvalidOperationException("只有方阵可以求逆。");
        }

        var n = _rows;
        var width = n * 2;
        var aug = new double[n * width];

        for (var r = 0; r < n; r++)
        {
            for (var c = 0; c < n; c++)
            {
                aug[(r * width) + c] = this[r, c];
                aug[(r * width) + (n + c)] = r == c ? 1d : 0d;
            }
        }

        const double eps = 1e-12;
        for (var pivot = 0; pivot < n; pivot++)
        {
            var maxRow = pivot;
            var maxVal = Math.Abs(aug[(pivot * width) + pivot]);
            for (var r = pivot + 1; r < n; r++)
            {
                var value = Math.Abs(aug[(r * width) + pivot]);
                if (value > maxVal)
                {
                    maxVal = value;
                    maxRow = r;
                }
            }

            if (maxVal < eps)
            {
                throw new InvalidOperationException("矩阵不可逆或接近奇异。");
            }

            if (maxRow != pivot)
            {
                SwapRows(aug, width, pivot, maxRow);
            }

            var pivotValue = aug[(pivot * width) + pivot];
            for (var c = 0; c < width; c++)
            {
                aug[(pivot * width) + c] /= pivotValue;
            }

            for (var r = 0; r < n; r++)
            {
                if (r == pivot)
                {
                    continue;
                }

                var factor = aug[(r * width) + pivot];
                if (Math.Abs(factor) < eps)
                {
                    continue;
                }

                for (var c = 0; c < width; c++)
                {
                    aug[(r * width) + c] -= factor * aug[(pivot * width) + c];
                }
            }
        }

        var result = CreateZero(n, n);
        for (var r = 0; r < n; r++)
        {
            for (var c = 0; c < n; c++)
            {
                result.SetValue(r, c, aug[(r * width) + (n + c)]);
            }
        }

        return result;
    }

    public Matrix Transpose()
    {
        var result = CreateZero(_cols, _rows);
        for (var r = 0; r < _rows; r++)
        {
            for (var c = 0; c < _cols; c++)
            {
                result.SetValue(c, r, this[r, c]);
            }
        }

        return result;
    }

    public string ToDisplayString(string format = "0.###")
    {
        var builder = new StringBuilder();
        for (var r = 0; r < _rows; r++)
        {
            for (var c = 0; c < _cols; c++)
            {
                if (c > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(this[r, c].ToString(format));
            }

            if (r < _rows - 1)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    public static Matrix operator +(Matrix left, Matrix right)
    {
        if (left._rows != right._rows || left._cols != right._cols)
        {
            throw new InvalidOperationException("矩阵加法要求行列相同。");
        }

        var result = CreateZero(left._rows, left._cols);
        for (var i = 0; i < left._values.Length; i++)
        {
            result._values[i] = left._values[i] + right._values[i];
        }

        return result;
    }

    public static Matrix operator -(Matrix left, Matrix right)
    {
        if (left._rows != right._rows || left._cols != right._cols)
        {
            throw new InvalidOperationException("矩阵减法要求行列相同。");
        }

        var result = CreateZero(left._rows, left._cols);
        for (var i = 0; i < left._values.Length; i++)
        {
            result._values[i] = left._values[i] - right._values[i];
        }

        return result;
    }

    public static Matrix operator *(Matrix left, Matrix right)
    {
        if (left._cols != right._rows)
        {
            throw new InvalidOperationException("矩阵乘法要求左矩阵列数等于右矩阵行数。");
        }

        var result = CreateZero(left._rows, right._cols);
        for (var r = 0; r < left._rows; r++)
        {
            for (var c = 0; c < right._cols; c++)
            {
                var sum = 0d;
                for (var k = 0; k < left._cols; k++)
                {
                    sum += left[r, k] * right[k, c];
                }

                result.SetValue(r, c, sum);
            }
        }

        return result;
    }

    private void ResizeStorage()
    {
        if (_rows <= 0 || _cols <= 0)
        {
            _values = Array.Empty<double>();
            return;
        }

        _values = new double[_rows * _cols];
    }

    private void ValidateIndex(int row, int col)
    {
        if (row < 0 || row >= _rows)
        {
            throw new ArgumentOutOfRangeException(nameof(row), "行索引超出范围。");
        }

        if (col < 0 || col >= _cols)
        {
            throw new ArgumentOutOfRangeException(nameof(col), "列索引超出范围。");
        }
    }

    private static void SwapRows(double[] data, int width, int rowA, int rowB)
    {
        if (rowA == rowB)
        {
            return;
        }

        var offsetA = rowA * width;
        var offsetB = rowB * width;
        for (var i = 0; i < width; i++)
        {
            (data[offsetA + i], data[offsetB + i]) = (data[offsetB + i], data[offsetA + i]);
        }
    }
}

