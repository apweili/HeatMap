using System.Windows;
using HeatMap;
using HeatMap.Enums;

namespace WpfApp1;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        HeatMap.MaxTemperature = 40;
        HeatMap.MinTemperature = 10;
        var maxPostion = 400;
        HeatMap.MaxVerticalPosition = maxPostion;
        HeatMap.MaxHorizontalPosition = maxPostion;
        HeatMap.Shape = Shape.Circle;
        HeatMap.TemperaturePoints =
        [
            new TemperaturePoint { X = 0, Y = 0, Temperature = 10 },
            new TemperaturePoint { X = HeatMap.MaxHorizontalPosition, Y = 0, Temperature = 30 },
            new TemperaturePoint { X = 0, Y = HeatMap.MaxVerticalPosition, Temperature = 20 },
            new TemperaturePoint
                { X = HeatMap.MaxHorizontalPosition, Y = HeatMap.MaxVerticalPosition, Temperature = 40 }
        ];
    }
}