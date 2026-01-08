using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Shape = HeatMap.Enums.Shape;
using Size = System.Windows.Size;

namespace HeatMap;

public class HeatMapVisualHost : UIElement
{
    public HeatMapVisualHost()
    {
        AddVisualChild(Background);
        AddVisualChild(DrawingHeatMapVisual);
    }

    private DrawingVisual DrawingHeatMapVisual { get; } = new();

    private Rectangle Background { get; } = new()
    {
        Fill = Brushes.Transparent
    };

    private bool HasPreparedToRenderHeatMap { get; set; }


    protected override Size MeasureCore(Size availableSize)
    {
        return availableSize;
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        base.ArrangeCore(finalRect);
        Background.Arrange(new Rect(new Point(0, 0), finalRect.Size));
        if (HasPreparedToRenderHeatMap) return;

        HasPreparedToRenderHeatMap = true;
        Dispatcher.InvokeAsync(RenderHeatMap);
    }

    private HeatMapSetting? HeatMapSetting { get; set; }

    public void SetHeatMap(HeatMapSetting heatMapSetting)
    {
        HeatMapSetting = heatMapSetting;
    }

    public void SetTemperaturePoints(IEnumerable<TemperaturePoint> temperaturePoints)
    {
        TemperaturePoints = temperaturePoints;
    }

    private IEnumerable<TemperaturePoint>? TemperaturePoints { get; set; }

    private void RenderHeatMap()
    {
        HasPreparedToRenderHeatMap = false;
        var width = Background.ActualWidth;
        var height = Background.ActualHeight;
        var maxPositionOnX = HeatMapSetting!.MaxHorizontalPosition;
        var maxPositionOnY = HeatMapSetting.MaxVerticalPosition;
        var getColorFromTemperature = HeatMapSetting.GetColorFromTemperature;
        var testPoints = TemperaturePoints!.Select(p => new TemperaturePoint
        {
            Temperature = p.Temperature,
            X = p.X / maxPositionOnX * width,
            Y = p.Y / maxPositionOnY * height
        }).Take(4).ToArray();
        // 定义四个点的坐标和温度值
        var p00 = testPoints[0];
        var p10 = testPoints[1];
        var p01 = testPoints[2];
        var p11 = testPoints[3];
        DrawingHeatMapVisual.Children.Clear();
        using var dc = DrawingHeatMapVisual.RenderOpen();
        if (HeatMapSetting.Shape == Shape.Circle)
        {
            var widthOffset = width / 2;
            var heightOffset = height / 2;
            var circleGeometry =
                new EllipseGeometry(new Point(widthOffset, heightOffset), widthOffset, heightOffset);
            dc.PushClip(circleGeometry);
        }

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                // 使用双线性插值计算当前点的温度
                var temperature = BilinearInterpolation(x, y, p00, p10, p01, p11);

                // 将温度映射到颜色
                var color = getColorFromTemperature(temperature);

                // 绘制像素
                dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(x, y, 1.5, 1.5));
            }
        }
    }

    private static double BilinearInterpolation(double x, double y, TemperaturePoint p00, TemperaturePoint p10,
        TemperaturePoint p01, TemperaturePoint p11)
    {
        var t = (x - p00.X) / (p10.X - p00.X);
        var a = p00.Temperature * (1 - t) + p10.Temperature * t;
        var b = p01.Temperature * (1 - t) + p11.Temperature * t;

        var u = (y - p00.Y) / (p01.Y - p00.Y);
        return a * (1 - u) + b * u;
    }

    protected override int VisualChildrenCount => 2;

    protected override Visual GetVisualChild(int index)
    {
        if (index == 1)
            return DrawingHeatMapVisual;

        return Background;
    }
}