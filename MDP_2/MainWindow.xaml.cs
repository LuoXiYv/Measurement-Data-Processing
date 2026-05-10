using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MDP_2.Models;
using MDP_2.Services;
using Microsoft.Win32;

namespace MDP_2;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    public ObservableCollection<LPointClass> Points { get; } = [];
    public ObservableCollection<LPointClass> CurrentPoints { get; } = [];
    public ObservableCollection<LineClass> Lines { get; } = [];

    private string _resultText = "请先点击“加载示例数据”或“导入CSV”，然后点击“计算”。";

    public string ResultText
    {
        get => _resultText;
        set
        {
            _resultText = value;
            OnPropertyChanged();
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void LoadSampleButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var csv = CsvDataLoader.GetEmbeddedExampleCsv();
            LoadData(csv);
            ResultText = "示例数据已加载，可直接点击“计算”。";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载示例失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportCsvButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV文件 (*.csv;*.txt)|*.csv;*.txt|所有文件 (*.*)|*.*",
            Title = "选择水准测量CSV数据文件"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var text = File.ReadAllText(dialog.FileName);
            LoadData(text);
            ResultText = $"已导入：{dialog.FileName}\n可点击“计算”开始处理。";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"导入失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ComputeButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var pointList = Points.ToList();
            var lineList = Lines.ToList();
            ResultText = LevelingAdjustmentService.Compute(pointList, lineList);

            RefreshCollection(Points, pointList);
            RefreshCollection(Lines, lineList);
            RefreshCurrentPoints(pointList);
            DrawProfile(pointList, lineList, useAdjusted: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"计算失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadData(string csvText)
    {
        var (points, lines) = CsvDataLoader.Parse(csvText);

        RefreshCollection(Points, points);
        RefreshCollection(Lines, lines);
        RefreshCurrentPoints(points);
        DrawProfile(points, lines, useAdjusted: false);
    }

    private static void RefreshCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void RefreshCurrentPoints(IEnumerable<LPointClass> points)
    {
        CurrentPoints.Clear();
        foreach (var p in points)
        {
            CurrentPoints.Add(new LPointClass
            {
                PID = p.PID,
                IsControlP = p.IsControlP,
                IsCommonP = p.IsCommonP,
                H = p.H,
                InitialH = p.InitialH,
                AdjustedH = p.AdjustedH,
                IsH0 = p.IsH0,
                X = p.X,
                Y = p.Y
            });
        }
    }

    private void DrawProfile(IReadOnlyList<LPointClass> points, IReadOnlyList<LineClass> lines, bool useAdjusted)
    {
        PlotCanvas.Children.Clear();

        if (points.Count == 0)
        {
            return;
        }

        var route = BuildPointOrder(lines, points.First().PID);
        if (route.Count == 0)
        {
            return;
        }

        var xs = new Dictionary<string, double> { [route[0]] = 0.0 };
        var totalDistance = 0.0;
        for (var i = 0; i < route.Count - 1; i++)
        {
            var line = lines.FirstOrDefault(l => (l.SPID == route[i] && l.EPID == route[i + 1]) || (l.EPID == route[i] && l.SPID == route[i + 1]));
            if (line is null)
            {
                continue;
            }

            totalDistance += line.Distance;
            xs[route[i + 1]] = totalDistance;
        }

        var displayPoints = points
            .Where(p => xs.ContainsKey(p.PID))
            .Select(p => new
            {
                p.PID,
                X = xs[p.PID],
                Y = useAdjusted ? p.AdjustedH : p.InitialH
            })
            .ToList();

        if (displayPoints.Count < 2)
        {
            return;
        }

        var minY = displayPoints.Min(p => p.Y);
        var maxY = displayPoints.Max(p => p.Y);
        if (Math.Abs(maxY - minY) < 1e-6)
        {
            maxY = minY + 1.0;
        }

        var w = Math.Max(PlotCanvas.ActualWidth, 300);
        var h = Math.Max(PlotCanvas.ActualHeight, 180);
        const double left = 40;
        const double right = 20;
        const double top = 20;
        const double bottom = 36;
        var xScale = (w - left - right) / Math.Max(totalDistance, 1.0);
        var yScale = (h - top - bottom) / (maxY - minY);

        var polyline = new Polyline
        {
            Stroke = Brushes.DarkOrange,
            StrokeThickness = 1.4
        };

        foreach (var p in displayPoints)
        {
            var x = left + p.X * xScale;
            var y = h - bottom - (p.Y - minY) * yScale;
            polyline.Points.Add(new Point(x, y));

            PlotCanvas.Children.Add(new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.ForestGreen,
                Stroke = Brushes.White,
                StrokeThickness = 0.8,
                Margin = new Thickness(x - 3, y - 3, 0, 0)
            });

            PlotCanvas.Children.Add(new TextBlock
            {
                Text = p.PID,
                Foreground = Brushes.DimGray,
                FontSize = 11,
                Margin = new Thickness(x - 8, y - 20, 0, 0)
            });
        }

        PlotCanvas.Children.Add(polyline);

        PlotCanvas.Children.Add(new TextBlock
        {
            Text = "距离 (km)",
            Foreground = Brushes.DimGray,
            FontSize = 11,
            Margin = new Thickness(w / 2 - 24, h - 20, 0, 0)
        });
        PlotCanvas.Children.Add(new TextBlock
        {
            Text = "高程 (m)",
            Foreground = Brushes.DimGray,
            FontSize = 11,
            Margin = new Thickness(2, h / 2 - 8, 0, 0)
        });
    }

    private static List<string> BuildPointOrder(IReadOnlyList<LineClass> lines, string startPid)
    {
        var order = new List<string> { startPid };
        var used = new HashSet<string>();
        var current = startPid;

        for (var i = 0; i < lines.Count + 2; i++)
        {
            var next = lines.FirstOrDefault(l => !used.Contains(l.LID) && l.SPID == current);
            if (next is null)
            {
                next = lines.FirstOrDefault(l => !used.Contains(l.LID) && l.EPID == current);
                if (next is null)
                {
                    break;
                }

                used.Add(next.LID);
                current = next.SPID;
                order.Add(current);
                continue;
            }

            used.Add(next.LID);
            current = next.EPID;
            order.Add(current);
        }

        return order;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}