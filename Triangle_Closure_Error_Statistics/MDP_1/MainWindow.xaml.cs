using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MDP_1.Models;
using MDP_1.Services;
using Microsoft.Win32;

namespace MDP_1;

public partial class MainWindow : Window
{
    private const double BinWidth = 0.2;
    private const double MaxAbsoluteValue = 2.6;

    private readonly TriangleClosureFileHandler _fileHandler = new();
    private readonly ObservableCollection<ClosureIntervalRow> _rows = new();

    private TriangleClosureStatistics? _statistics;

    public MainWindow()
    {
        InitializeComponent();
        StatisticsDataGrid.ItemsSource = _rows;
    }

    private void LoadDataButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择闭合差数据文件",
            Filter = "Data files (*.csv;*.txt)|*.csv;*.txt|All files (*.*)|*.*",
            InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            _fileHandler.ReadTriangleClosureDifferences(dialog.FileName);
            _statistics = null;
            _rows.Clear();
            ChartCanvas.Children.Clear();

            SummaryTextBlock.Text = string.Create(
                CultureInfo.InvariantCulture,
                $"已读取 {_fileHandler.TriangleClosureDifferences.Count} 条闭合差数据。请点击【统计计算】。\n当前文件: {dialog.FileName}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"读取失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_fileHandler.TriangleClosureDifferences.Count == 0)
        {
            MessageBox.Show("请先读取数据文件。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _statistics = TriangleClosureStatistics.CreateFromValues(
            _fileHandler.TriangleClosureDifferences,
            BinWidth,
            MaxAbsoluteValue);

        _rows.Clear();
        foreach (var row in _statistics.Rows)
        {
            _rows.Add(row);
        }

        SummaryTextBlock.Text = string.Create(
            CultureInfo.InvariantCulture,
            $"统计完成: -Δ总数={_statistics.NegativeTotalCount}, +Δ总数={_statistics.PositiveTotalCount}, " +
            $"-Δ频率和={_statistics.SumNegativeFrequencies():F6}, +Δ频率和={_statistics.SumPositiveFrequencies():F6}");

        DrawFrequencyChart(_statistics);
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_statistics is null)
        {
            MessageBox.Show("请先完成统计计算。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "保存统计结果",
            Filter = "CSV 文件 (*.csv)|*.csv|文本文件 (*.txt)|*.txt",
            FileName = "triangle_closure_statistics.csv",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            _fileHandler.WriteStatisticsResult(dialog.FileName, _statistics);
            MessageBox.Show($"导出成功: {dialog.FileName}", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DrawFrequencyChart(TriangleClosureStatistics statistics)
    {
        ChartCanvas.Children.Clear();

        var width = ChartCanvas.ActualWidth;
        var height = ChartCanvas.ActualHeight;

        if (width < 100 || height < 80)
        {
            width = ChartCanvas.Width > 0 ? ChartCanvas.Width : 900;
            height = ChartCanvas.Height > 0 ? ChartCanvas.Height : 220;
        }

        var marginLeft = 44.0;
        var marginRight = 20.0;
        var marginTop = 18.0;
        var marginBottom = 34.0;

        var plotWidth = width - marginLeft - marginRight;
        var plotHeight = height - marginTop - marginBottom;

        if (plotWidth <= 0 || plotHeight <= 0)
        {
            return;
        }

        var rows = statistics.Rows.Where(r => !r.IntervalLabel.StartsWith(">", StringComparison.Ordinal)).ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var maxFreq = rows.SelectMany(r => new[] { r.NegativeFrequency, r.PositiveFrequency }).DefaultIfEmpty(0).Max();
        if (maxFreq <= 0)
        {
            maxFreq = 1;
        }

        var xAxis = new Line
        {
            X1 = marginLeft,
            Y1 = marginTop + plotHeight,
            X2 = marginLeft + plotWidth,
            Y2 = marginTop + plotHeight,
            Stroke = Brushes.Black,
            StrokeThickness = 1,
        };
        var yAxis = new Line
        {
            X1 = marginLeft,
            Y1 = marginTop,
            X2 = marginLeft,
            Y2 = marginTop + plotHeight,
            Stroke = Brushes.Black,
            StrokeThickness = 1,
        };

        ChartCanvas.Children.Add(xAxis);
        ChartCanvas.Children.Add(yAxis);

        var negativePolyline = new Polyline
        {
            Stroke = Brushes.IndianRed,
            StrokeThickness = 2,
        };

        var positivePolyline = new Polyline
        {
            Stroke = Brushes.SteelBlue,
            StrokeThickness = 2,
        };

        for (var i = 0; i < rows.Count; i++)
        {
            var x = marginLeft + (i * plotWidth / Math.Max(1, rows.Count - 1));
            var negativeY = marginTop + plotHeight - (rows[i].NegativeFrequency / maxFreq * plotHeight);
            var positiveY = marginTop + plotHeight - (rows[i].PositiveFrequency / maxFreq * plotHeight);

            negativePolyline.Points.Add(new Point(x, negativeY));
            positivePolyline.Points.Add(new Point(x, positiveY));

            if (i % 2 == 0 || i == rows.Count - 1)
            {
                var tick = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = marginTop + plotHeight,
                    Y2 = marginTop + plotHeight + 5,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                };
                ChartCanvas.Children.Add(tick);

                var label = new TextBlock
                {
                    Text = rows[i].IntervalLabel,
                    FontSize = 10,
                };
                Canvas.SetLeft(label, x - 18);
                Canvas.SetTop(label, marginTop + plotHeight + 6);
                ChartCanvas.Children.Add(label);
            }
        }

        ChartCanvas.Children.Add(negativePolyline);
        ChartCanvas.Children.Add(positivePolyline);

        var title = new TextBlock
        {
            Text = "频率折线图: 红色=-Δ, 蓝色=+Δ",
            FontWeight = FontWeights.SemiBold,
        };
        Canvas.SetLeft(title, marginLeft + 6);
        Canvas.SetTop(title, 0);
        ChartCanvas.Children.Add(title);
    }
}