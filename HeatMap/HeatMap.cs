using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HeatMap;

[TemplatePart(Name = HeatMapVisualHostTemplateName, Type = typeof(HeatMapVisualHost))]
[TemplatePart(Name = CoordinateSystemCanvasName, Type = typeof(Canvas))]
public partial class HeatMap : Control
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

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HeatMapVisualHost = (HeatMapVisualHost)GetTemplateChild(HeatMapVisualHostTemplateName)!;
        CoordinateSystemCanvas = (Canvas)GetTemplateChild(CoordinateSystemCanvasName)!;
        CoordinateSystemCanvas.SizeChanged -= CoordinateSystemCanvasOnSizeChanged;
        CoordinateSystemCanvas.SizeChanged += CoordinateSystemCanvasOnSizeChanged;
        HeatMapVisualHost.SetHeatMap(new HeatMapSetting(MaxHorizontalPosition, MaxVerticalPosition,
            GetColorFromTemperature, Shape));
        HeatMapVisualHost.SetTemperaturePoints(TemperaturePoints);
    }

    private void CoordinateSystemCanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var coordinateSystemCanvas = (Canvas)sender;
        coordinateSystemCanvas.Children.Clear();
        DrawCoordinateSystem(coordinateSystemCanvas, e.NewSize.Width, e.NewSize.Height);
    }

    private void DrawCoordinateSystem(Canvas canvas, double width, double height)
    {
        const double fontSizeFactor = 10d / 200;
        var maxTemperature = MaxTemperature;
        var minTemperature = MinTemperature;
        var rectangleWidth = width / 8;
        canvas.Children.Add(CreateRectangle(rectangleWidth, height, minTemperature, maxTemperature));
        var temperatureSpan = maxTemperature - minTemperature;
        var tickLineHorizonOffset = rectangleWidth / 3;
        var fontSize = fontSizeFactor * height;
        var textSize = CalculateTextBlockHeight("A", fontSize);
        foreach (var temperaturePoint in TemperaturePoints)
        {
            var tickHeight = height - (temperaturePoint.Temperature - minTemperature) / temperatureSpan * height;
            if (tickHeight == 0)
            {
                tickHeight = 2;
            }

            var tickLine = new Line
            {
                X1 = tickLineHorizonOffset,
                Y1 = tickHeight,
                X2 = rectangleWidth,
                Y2 = tickHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            canvas.Children.Add(tickLine);
            var label = new TextBlock
            {
                Text = GetTemperaturePointInfo(temperaturePoint),
                Foreground = Brushes.Black,
                FontSize = fontSize,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Canvas.SetLeft(label, rectangleWidth);
            var textBlockHeight = tickHeight - textSize / 4;
            if (textBlockHeight + textSize > height)
            {
                textBlockHeight = tickHeight - textSize;
            }
            Canvas.SetTop(label, textBlockHeight);
            canvas.Children.Add(label);
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
        return GetColorFromTemperature(temperature, MinTemperature, MaxTemperature);
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

    private static string GetTemperaturePointInfo(TemperaturePoint point)
    {
        return $"{point.X}-{point.Y} {point.Temperature}";
    }

    private double CalculateTextBlockHeight(string labelText, double fontSize)
    {
        var pixelsPerDip = SystemParameters.FullPrimaryScreenWidth / SystemParameters.PrimaryScreenWidth;
        var formattedText = new FormattedText(labelText, System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), fontSize, Brushes.Black, pixelsPerDip);
        return formattedText.Height;
    }
}