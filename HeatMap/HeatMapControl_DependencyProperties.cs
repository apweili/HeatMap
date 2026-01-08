using System.Windows;
using HeatMap.Enums;

namespace HeatMap;

public partial class HeatMapControl
{
    private const double DefaultMaxPosition = 400;
    private const double DefaultMinTemperature = 10;
    private const double DefaultMaxTemperature = 40;

    public static readonly DependencyProperty MinTemperatureProperty =
        DependencyProperty.Register(nameof(MinTemperature), typeof(double), typeof(HeatMapControl),
            new PropertyMetadata(DefaultMinTemperature));

    public double MinTemperature
    {
        get => (double)GetValue(MinTemperatureProperty);
        set => SetValue(MinTemperatureProperty, value);
    }

    public static readonly DependencyProperty MaxTemperatureProperty =
        DependencyProperty.Register(nameof(MaxTemperature), typeof(double), typeof(HeatMapControl),
            new PropertyMetadata(DefaultMaxTemperature));

    public double MaxTemperature
    {
        get => (double)GetValue(MaxTemperatureProperty);
        set => SetValue(MaxTemperatureProperty, value);
    }

    public static readonly DependencyProperty MaxHorizontalPositionProperty =
        DependencyProperty.Register(nameof(MaxHorizontalPosition), typeof(double), typeof(HeatMapControl),
            new PropertyMetadata(DefaultMaxPosition));

    public double MaxHorizontalPosition
    {
        get => (double)GetValue(MaxHorizontalPositionProperty);
        set => SetValue(MaxHorizontalPositionProperty, value);
    }

    public static readonly DependencyProperty MaxVerticalPositionProperty =
        DependencyProperty.Register(nameof(MaxVerticalPosition), typeof(double), typeof(HeatMapControl),
            new PropertyMetadata(DefaultMaxPosition));

    public double MaxVerticalPosition
    {
        get => (double)GetValue(MaxVerticalPositionProperty);
        set => SetValue(MaxVerticalPositionProperty, value);
    }

    public static readonly DependencyProperty TemperaturePointsProperty =
        DependencyProperty.Register(nameof(TemperaturePoints), typeof(IEnumerable<TemperaturePoint>), typeof(HeatMapControl),
            new PropertyMetadata(null));

    public IEnumerable<TemperaturePoint> TemperaturePoints
    {
        get => (IEnumerable<TemperaturePoint>)GetValue(TemperaturePointsProperty);
        set => SetValue(TemperaturePointsProperty, value);
    }

    public static readonly DependencyProperty ShapeProperty =
        DependencyProperty.Register(nameof(Shape), typeof(Shape), typeof(HeatMapControl),
            new PropertyMetadata(Shape.Circle));

    public Shape Shape
    {
        get => (Shape)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }
}