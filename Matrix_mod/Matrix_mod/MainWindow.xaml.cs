using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Matrix_mod;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void RandomAButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryGetSize(ARowsTextBox.Text, AColsTextBox.Text, out var rows, out var cols, out var error))
        {
            SetStatus(error, true);
            return;
        }

        var matrix = new Matrix(rows, cols);
        MatrixATextBox.Text = matrix.ToDisplayString();
        SetStatus($"已生成 {rows}x{cols} 的矩阵 A。", false);
    }

    private void RandomBButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryGetSize(BRowsTextBox.Text, BColsTextBox.Text, out var rows, out var cols, out var error))
        {
            SetStatus(error, true);
            return;
        }

        var matrix = new Matrix(rows, cols);
        MatrixBTextBox.Text = matrix.ToDisplayString();
        SetStatus($"已生成 {rows}x{cols} 的矩阵 B。", false);
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryParseMatrix(MatrixATextBox, "A", out var a) || !TryParseMatrix(MatrixBTextBox, "B", out var b))
        {
            return;
        }

        try
        {
            var result = a + b;
            ResultTextBox.Text = result.ToDisplayString();
            SetStatus("已完成 A + B。", false);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, true);
        }
    }

    private void SubtractButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryParseMatrix(MatrixATextBox, "A", out var a) || !TryParseMatrix(MatrixBTextBox, "B", out var b))
        {
            return;
        }

        try
        {
            var result = a - b;
            ResultTextBox.Text = result.ToDisplayString();
            SetStatus("已完成 A - B。", false);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, true);
        }
    }

    private void MultiplyButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryParseMatrix(MatrixATextBox, "A", out var a) || !TryParseMatrix(MatrixBTextBox, "B", out var b))
        {
            return;
        }

        try
        {
            var result = a * b;
            ResultTextBox.Text = result.ToDisplayString();
            SetStatus("已完成 A × B。", false);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, true);
        }
    }

    private void InverseAButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryParseMatrix(MatrixATextBox, "A", out var a))
        {
            return;
        }

        try
        {
            var result = a.Inverse();
            ResultTextBox.Text = result.ToDisplayString();
            SetStatus("已完成 A 的逆矩阵。", false);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, true);
        }
    }

    private void InverseBButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryParseMatrix(MatrixBTextBox, "B", out var b))
        {
            return;
        }

        try
        {
            var result = b.Inverse();
            ResultTextBox.Text = result.ToDisplayString();
            SetStatus("已完成 B 的逆矩阵。", false);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, true);
        }
    }

    private void ClearButton_OnClick(object sender, RoutedEventArgs e)
    {
        MatrixATextBox.Clear();
        MatrixBTextBox.Clear();
        ResultTextBox.Clear();
        SetStatus("已清空输入与结果。", false);
    }

    private bool TryParseMatrix(TextBox textBox, string name, [NotNullWhen(true)] out Matrix? matrix)
    {
        if (MatrixParser.TryParse(textBox.Text, out matrix, out var error))
        {
            return true;
        }

        SetStatus($"矩阵 {name} 解析失败：{error}", true);
        return false;
    }

    private static bool TryGetSize(string rowText, string colText, out int rows, out int cols, out string error)
    {
        error = string.Empty;
        rows = 0;
        cols = 0;

        if (!int.TryParse(rowText, out rows) || rows <= 0)
        {
            error = "请输入有效的行数。";
            return false;
        }

        if (!int.TryParse(colText, out cols) || cols <= 0)
        {
            error = "请输入有效的列数。";
            return false;
        }

        return true;
    }

    private void SetStatus(string message, bool isError)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = isError ? Brushes.IndianRed : Brushes.DarkGreen;
    }
}