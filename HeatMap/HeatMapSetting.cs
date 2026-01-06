using System.Windows.Media;

namespace HeatMap;

public class HeatMapSetting(
    double maxPositionOnX,
    double maxPositionOnY,
    Func<double, Color> getColorFromTemperature)
{
    public double MaxPositionOnX { get; } = maxPositionOnX;
    public double MaxPositionOnY { get; } = maxPositionOnY;
    public Func<double, Color> GetColorFromTemperature { get; } = getColorFromTemperature;
}