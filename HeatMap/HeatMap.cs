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
    private readonly double _maxPosition = 400;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HeatMapVisualHost = (HeatMapVisualHost)GetTemplateChild(HeatMapVisualHostTemplateName)!;
        CoordinateSystemCanvas = (Canvas)GetTemplateChild(CoordinateSystemCanvasName)!;
        CoordinateSystemCanvas.SizeChanged -= CoordinateSystemCanvasOnSizeChanged;
        CoordinateSystemCanvas.SizeChanged += CoordinateSystemCanvasOnSizeChanged;
        HeatMapVisualHost.SetHeatMap(new HeatMapSetting(_maxPosition, _maxPosition, GetColorFromTemperature));
        HeatMapVisualHost.SetTemperaturePoints([
            new TemperaturePoint { X = 0, Y = 0, Temperature = 10 },
            new TemperaturePoint { X = _maxPosition, Y = 0, Temperature = 30 },
            new TemperaturePoint { X = 0, Y = _maxPosition, Temperature = 20 },
            new TemperaturePoint { X = _maxPosition, Y = _maxPosition, Temperature = 40 }
        ]);
    }

    private void CoordinateSystemCanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var coordinateSystemCanvas = (Canvas)sender;
        coordinateSystemCanvas.Children.Clear();
        DepictCoordinateSystem(coordinateSystemCanvas, e.NewSize.Width, e.NewSize.Height);
    }

    private void DepictCoordinateSystem(Canvas canvas, double width, double height)
    {
        canvas.Children.Add(CreateRectangle());
        return;

        Rectangle CreateRectangle()
        {
            var rectangle = new Rectangle
            {
                Width = width / 5,
                Height = height,
                Fill = CreateLinearGradientBrush()
            };

            return rectangle;
        }

        LinearGradientBrush CreateLinearGradientBrush()
        {
            var linearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };

            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Navy, 0));
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Navy, 0.25));
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Green, 0.26));
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Green, 0.50));
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.51));
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.75));
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 0.76));
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 1));
            return linearGradientBrush;
        }
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