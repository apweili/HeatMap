using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HeatMap;

[TemplatePart(Name = HeatMapVisualHostTemplateName, Type = typeof(HeatMapVisualHost))]
[TemplatePart(Name = CoordinateSystemCanvasName, Type = typeof(Canvas))]
[TemplatePart(Name = CoordinateXCanvasName, Type = typeof(Canvas))]
[TemplatePart(Name = CoordinateYCanvasName, Type = typeof(Canvas))]
public partial class HeatMapControl : Control
{
    static HeatMapControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HeatMapControl),
            new FrameworkPropertyMetadata(typeof(HeatMapControl)));
        SnapsToDevicePixelsProperty.OverrideMetadata(typeof(HeatMapControl), new FrameworkPropertyMetadata(true));
    }


    private static readonly RoutedCommand SaveImageCommand = new("SaveImage", typeof(HeatMapControl));

    public HeatMapControl()
    {
        FocusManager.SetIsFocusScope(this, true);
        FocusManager.SetFocusedElement(this, this);
        ContextMenu = CreateContextMenu();
        CommandBindings.Add(new CommandBinding(SaveImageCommand, OnSaveImageExecuted));
    }

    private const string HeatMapVisualHostTemplateName = "PART_HeatMapVisualHost";
    private const string CoordinateSystemCanvasName = "PART_CoordinateSystemCanvas";
    private const string CoordinateXCanvasName = "PART_CoordinateXCanvas";
    private const string CoordinateYCanvasName = "PART_CoordinateYCanvas";

    private HeatMapVisualHost? HeatMapVisualHost { get; set; }
    private Canvas? CoordinateSystemCanvas { get; set; }
    private Canvas? CoordinateXCanvas { get; set; }
    private Canvas? CoordinateYCanvas { get; set; }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HeatMapVisualHost = (HeatMapVisualHost)GetTemplateChild(HeatMapVisualHostTemplateName)!;
        HeatMapVisualHost.SetHeatMap(new HeatMapSetting(MaxHorizontalPosition, MaxVerticalPosition,
            GetColorFromTemperature, Shape));
        HeatMapVisualHost.SetTemperaturePoints(TemperaturePoints);

        CoordinateSystemCanvas = (Canvas)GetTemplateChild(CoordinateSystemCanvasName)!;
        CoordinateSystemCanvas.SizeChanged -= CoordinateSystemCanvasOnSizeChanged;
        CoordinateSystemCanvas.SizeChanged += CoordinateSystemCanvasOnSizeChanged;

        CoordinateXCanvas = (Canvas)GetTemplateChild(CoordinateXCanvasName)!;
        CoordinateXCanvas.SizeChanged -= CoordinateXCanvasOnSizeChanged;
        CoordinateXCanvas.SizeChanged += CoordinateXCanvasOnSizeChanged;

        CoordinateYCanvas = (Canvas)GetTemplateChild(CoordinateYCanvasName)!;
        CoordinateYCanvas.SizeChanged -= CoordinateYCanvasOnSizeChanged;
        CoordinateYCanvas.SizeChanged += CoordinateYCanvasOnSizeChanged;

        AddHandlersForPopup();
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        Dispatcher.InvokeAsync(() => Keyboard.Focus(this));
    }

    private void CoordinateYCanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var coordinateXCanvas = (Canvas)sender;
        coordinateXCanvas.Children.Clear();
        DrawCoordinateYCanvas(coordinateXCanvas, e.NewSize.Width, e.NewSize.Height);
    }

    private void CoordinateXCanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var coordinateXCanvas = (Canvas)sender;
        coordinateXCanvas.Children.Clear();
        DrawCoordinateXCanvas(coordinateXCanvas, e.NewSize.Width, e.NewSize.Height);
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
        var (_, textHeight) = CalculateTextBlockHeight("A", fontSize);
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
            var textBlockHeight = tickHeight - textHeight / 4;
            if (textBlockHeight + textHeight > height)
            {
                textBlockHeight = tickHeight - textHeight;
            }

            Canvas.SetTop(label, textBlockHeight);
            canvas.Children.Add(label);
        }
    }


    private void DrawCoordinateYCanvas(Canvas coordinateYCanvas, double newSizeWidth, double newSizeHeight)
    {
        var lineXOffset = newSizeWidth * 3 / 4;
        var axisLine = new Line
        {
            X1 = lineXOffset,
            Y1 = 0,
            X2 = lineXOffset,
            Y2 = newSizeHeight,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        coordinateYCanvas.Children.Add(axisLine);

        const int numTicksPerDirection = 2;
        const int numTicks = numTicksPerDirection * 2;
        var start = -MaxVerticalPosition / 2;
        var end = MaxVerticalPosition / 2;
        const double fontSizeFactor = 0.2;
        var font = fontSizeFactor * newSizeWidth;
        var tickInterval = (end - start) / numTicks;
        var textBlockEndOffset = lineXOffset * 0.9;
        var tickMarkLength = (newSizeWidth - lineXOffset) * 0.6;
        for (var i = 0; i <= numTicks; i++)
        {
            var position = i * tickInterval;
            var yPosition = newSizeHeight - position / MaxVerticalPosition * newSizeHeight;
            var tickMark = new Line
            {
                X1 = lineXOffset,
                Y1 = yPosition,
                X2 = lineXOffset + tickMarkLength,
                Y2 = yPosition,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            coordinateYCanvas.Children.Add(tickMark);

            var label = new TextBlock
            {
                Text = (start + position).ToString("F2"),
                Foreground = Brushes.Black,
                FontSize = font,
            };

            var textBlockXOffset = textBlockEndOffset - CalculateTextBlockHeight(label.Text, font).Width;
            Canvas.SetLeft(label, textBlockXOffset);
            Canvas.SetTop(label, yPosition - font * i switch
            {
                numTicksPerDirection => 1,
                numTicks => 0,
                _ => 1.2
            });
            coordinateYCanvas.Children.Add(label);
        }
    }

    private void DrawCoordinateXCanvas(Canvas coordinateXCanvas, double newSizeWidth, double newSizeHeight)
    {
        var lineYOffset = newSizeHeight / 4;
        var axisLine = new Line
        {
            X1 = 0,
            Y1 = lineYOffset,
            X2 = newSizeWidth,
            Y2 = lineYOffset,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        coordinateXCanvas.Children.Add(axisLine);

        const int numTicksPerDirection = 2;
        const int numTicks = numTicksPerDirection * 2;
        var start = -MaxHorizontalPosition / 2;
        var end = MaxHorizontalPosition / 2;
        const double fontSizeFactor = 0.3;
        var font = fontSizeFactor * newSizeHeight;
        var tickInterval = (end - start) / numTicks;
        var tickMarkY1 = lineYOffset * 0.2;
        var textBlockYOffset = lineYOffset * 1.2;
        for (var i = 0; i <= numTicks; i++)
        {
            var position = i * tickInterval;
            var xPosition = position / MaxHorizontalPosition * newSizeWidth;
            var tickMark = new Line
            {
                X1 = xPosition,
                Y1 = tickMarkY1,
                X2 = xPosition,
                Y2 = lineYOffset,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            coordinateXCanvas.Children.Add(tickMark);

            var label = new TextBlock
            {
                Text = (start + position).ToString("F2"),
                Foreground = Brushes.Black,
                FontSize = font,
            };
            Canvas.SetLeft(label, xPosition - font * i switch
            {
                numTicksPerDirection => 1,
                numTicks => 2.6,
                _ => 2
            });
            Canvas.SetTop(label, textBlockYOffset);
            coordinateXCanvas.Children.Add(label);
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
        var colorUsedCount = 0;
        for (double offset = 0; offset < 1; offset += offsetSpan, colorUsedCount++)
        {
            if (colorUsedCount == 2)
            {
                colorUsedCount = 0;
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

    private (double Width, double Height) CalculateTextBlockHeight(string labelText, double fontSize)
    {
        var pixelsPerDip = SystemParameters.FullPrimaryScreenWidth / SystemParameters.PrimaryScreenWidth;
        var formattedText = new FormattedText(labelText, System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), fontSize, Brushes.Black, pixelsPerDip);
        return (formattedText.Width, formattedText.Height);
    }
}