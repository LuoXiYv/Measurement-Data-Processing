using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MDP_2.Models;

public class LPointClass : INotifyPropertyChanged
{
    private double _h;
    private double _initialH;
    private double _adjustedH;

    public string PID { get; set; } = string.Empty;
    public double H
    {
        get => _h;
        set
        {
            _h = value;
            OnPropertyChanged();
        }
    }

    public double InitialH
    {
        get => _initialH;
        set
        {
            _initialH = value;
            OnPropertyChanged();
        }
    }

    public double AdjustedH
    {
        get => _adjustedH;
        set
        {
            _adjustedH = value;
            OnPropertyChanged();
        }
    }

    public bool IsControlP { get; set; }
    public bool IsH0 { get; set; }
    public bool IsCommonP { get; set; }
    
    public double X { get; set; }
    public double Y { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

