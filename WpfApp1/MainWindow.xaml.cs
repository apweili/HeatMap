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
        const int maxPosition = 400;
        HeatMap.MaxVerticalPosition = maxPosition;
        HeatMap.MaxHorizontalPosition = maxPosition;
        HeatMap.Shape = Shape.Circle;
        HeatMap.TemperaturePoints =
        [
            new TemperaturePoint
            {
                X = 0 - HeatMap.MaxHorizontalPosition / 2, Y = 0 - HeatMap.MaxVerticalPosition / 2, Z = 111,
                Temperature = 10
            },
            new TemperaturePoint
            {
                X = HeatMap.MaxHorizontalPosition - HeatMap.MaxHorizontalPosition / 2,
                Y = 0 - HeatMap.MaxVerticalPosition / 2, Z = 333, Temperature = 30
            },
            new TemperaturePoint
            {
                X = 0 - HeatMap.MaxHorizontalPosition / 2,
                Y = HeatMap.MaxVerticalPosition - HeatMap.MaxVerticalPosition / 2, Z = 222, Temperature = 20
            },
            new TemperaturePoint
            {
                X = HeatMap.MaxHorizontalPosition - HeatMap.MaxHorizontalPosition / 2,
                Y = HeatMap.MaxVerticalPosition - HeatMap.MaxVerticalPosition / 2,
                Z = 444, Temperature = 40
            },
            new TemperaturePoint
            {
                X = HeatMap.MaxVerticalPosition / 2 - HeatMap.MaxHorizontalPosition / 2,
                Y = HeatMap.MaxVerticalPosition / 2 - HeatMap.MaxVerticalPosition / 2, Z = 111, Temperature = 10
            },
        ];
    }
}