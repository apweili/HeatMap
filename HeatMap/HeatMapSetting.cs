using System.Windows.Media;
using HeatMap.Enums;

namespace HeatMap;

public class HeatMapSetting(
    double maxHorizontalPosition,
    double maxVerticalPosition,
    Func<double, Color> getColorFromTemperature,
    Shape shape)
{
    public double MaxHorizontalPosition { get; } = maxHorizontalPosition;
    public double MaxVerticalPosition { get; } = maxVerticalPosition;
    public Shape Shape { get; } = shape;
    public Func<double, Color> GetColorFromTemperature { get; } = getColorFromTemperature;
}