using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CylindricalTopSurface;

[TemplatePart(Name = HeatMapVisualHostTemplateName, Type = typeof(Canvas))]
public class HeatMap : Control
{
    static HeatMap()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HeatMap),
            new FrameworkPropertyMetadata(typeof(HeatMap)));
        SnapsToDevicePixelsProperty.OverrideMetadata(typeof(HeatMap), new FrameworkPropertyMetadata(true));
    }

    private const string HeatMapVisualHostTemplateName = "PART_HeatMapVisualHost";

    private HeatMapVisualHost? HeatMapVisualHost { get; set; }

    private readonly double _minTemp = 10;
    private readonly double _maxTemp = 40;
    private readonly double _maxPosition = 400;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HeatMapVisualHost = (HeatMapVisualHost)GetTemplateChild(HeatMapVisualHostTemplateName)!;
        HeatMapVisualHost.SetHeatMap(new HeatMapSetting(_maxPosition, _maxPosition, GetColorFromTemperature));
        HeatMapVisualHost.SetTemperaturePoints([
            new TemperaturePoint { X = 0, Y = 0, Temperature = 10 },
            new TemperaturePoint { X = _maxPosition, Y = 0, Temperature = 30 },
            new TemperaturePoint { X = 0, Y = _maxPosition, Temperature = 20 },
            new TemperaturePoint { X = _maxPosition, Y = _maxPosition, Temperature = 40 }
        ]);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        HeatMapVisualHost!.Measure(new Size(constraint.Width / 5 * 4, constraint.Height));
        return new Size(HeatMapVisualHost.DesiredSize.Width + HeatMapVisualHost.DesiredSize.Width / 4,
            HeatMapVisualHost.DesiredSize.Height);
    }

    private Color GetColorFromTemperature(double temperature)
    {
        return GetColorFromTemperature(temperature, _minTemp, _maxTemp);
    }

    private static Color GetColorFromTemperature(double temperature, double minTemp, double maxTemp)
    {
        // 将温度映射到0-1之间
        var normalizedTemp = (temperature - minTemp) / (maxTemp - minTemp);

        // 使用蓝色到红色的渐变
        var red = (byte)(255 * normalizedTemp);
        byte green = 0;
        var blue = (byte)(255 * (1 - normalizedTemp));

        return Color.FromRgb(red, green, blue);
    }
}