using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HeatMap;

[TemplatePart(Name = HeatMapVisualHostTemplateName, Type = typeof(HeatMapVisualHost))]
[TemplatePart(Name = CoordinateSystemCanvasName, Type = typeof(Canvas))]
public class HeatMap : Control
{
    static HeatMap()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HeatMap),
            new FrameworkPropertyMetadata(typeof(HeatMap)));
        SnapsToDevicePixelsProperty.OverrideMetadata(typeof(HeatMap), new FrameworkPropertyMetadata(true));
    }

    private const string HeatMapVisualHostTemplateName = "PART_HeatMapVisualHost";
    private const string CoordinateSystemCanvasName = "PART_CoordinateSystemCanvas";

    private HeatMapVisualHost? HeatMapVisualHost { get; set; }
    private Canvas? CoordinateSystemCanvas { get; set; }

    private readonly double _minTemp = 10;
    private readonly double _maxTemp = 40;
    private readonly double _maxPosition = DefaultMaxPosition;

    private const double DefaultMaxPosition = 400;

    private IEnumerable<TemperaturePoint> TemperaturePoints { get; } =
    [
        new() { X = 0, Y = 0, Temperature = 10 },
        new() { X = DefaultMaxPosition, Y = 0, Temperature = 30 },
        new() { X = 0, Y = DefaultMaxPosition, Temperature = 20 },
        new() { X = DefaultMaxPosition, Y = DefaultMaxPosition, Temperature = 40 }
    ];

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HeatMapVisualHost = (HeatMapVisualHost)GetTemplateChild(HeatMapVisualHostTemplateName)!;
        CoordinateSystemCanvas = (Canvas)GetTemplateChild(CoordinateSystemCanvasName)!;
        CoordinateSystemCanvas.SizeChanged -= CoordinateSystemCanvasOnSizeChanged;
        CoordinateSystemCanvas.SizeChanged += CoordinateSystemCanvasOnSizeChanged;
        HeatMapVisualHost.SetHeatMap(new HeatMapSetting(_maxPosition, _maxPosition, GetColorFromTemperature));
        HeatMapVisualHost.SetTemperaturePoints(TemperaturePoints);
    }

    private void CoordinateSystemCanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var coordinateSystemCanvas = (Canvas)sender;
        coordinateSystemCanvas.Children.Clear();
        DepictCoordinateSystem(coordinateSystemCanvas, e.NewSize.Width, e.NewSize.Height);
    }

    private void DepictCoordinateSystem(Canvas canvas, double width, double height)
    {
        var rectangleWidth = width / 5;
        canvas.Children.Add(CreateRectangle(rectangleWidth, height, _minTemp, _maxTemp));
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

    private static Rectangle CreateRectangle(double width, double height, double minTemp, double maxTemp)
    {
        var rectangle = new Rectangle
        {
            Width = width,
            Height = height,
            Fill = CreateLinearGradientBrush(height, 2, minTemp, maxTemp)
        };

        return rectangle;
    }

    private static LinearGradientBrush CreateLinearGradientBrush(double height, double heightSpan, double minTemp,
        double maxTemp)
    {
        var linearGradientBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 1),
            EndPoint = new Point(0, 0)
        };

        var offsetSpan = heightSpan / height;
        var color = GetColorFromTemperature(minTemp, minTemp, maxTemp);
        var colorUsedCountd = 0;
        for (double offset = 0; offset < 1; offset += offsetSpan, colorUsedCountd++)
        {
            if (colorUsedCountd == 2)
            {
                colorUsedCountd = 0;
                var temperature = minTemp + offset * (maxTemp - minTemp);
                color = GetColorFromTemperature(temperature, minTemp, maxTemp);
            }

            linearGradientBrush.GradientStops.Add(new GradientStop(color, offset));
        }

        color = GetColorFromTemperature(maxTemp, minTemp, maxTemp);
        linearGradientBrush.GradientStops.Add(new GradientStop(color, 1));
        return linearGradientBrush;
    }
}