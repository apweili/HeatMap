using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Shape = HeatMap.Enums.Shape;
using Size = System.Windows.Size;

namespace HeatMap;

public class HeatMapVisualHost : UIElement
{
    private readonly VisualCollection _visualCollection;

    public HeatMapVisualHost()
    {
        _visualCollection = new VisualCollection(this)
        {
            Background,
            DrawingHeatMapVisual
        };
    }

    private DrawingVisual DrawingHeatMapVisual { get; } = new();
    private DrawingVisual DrawingPointsVisual { get; } = new();

    public Rectangle Background { get; } = new()
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
        var renderWidth = Background.ActualWidth;
        var renderHeight = Background.ActualHeight;
        var maxPositionOnX = HeatMapSetting!.MaxHorizontalPosition;
        var maxPositionOnY = HeatMapSetting.MaxVerticalPosition;
        var getColorFromTemperature = HeatMapSetting.GetColorFromTemperature;
        var testPoints = TemperaturePoints!.Select(p => new TemperaturePoint
        {
            Temperature = p.Temperature,
            X = p.X / maxPositionOnX * renderWidth,
            Y = p.Y / maxPositionOnY * renderHeight
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
            var widthOffset = renderWidth / 2;
            var heightOffset = renderHeight / 2;
            var circleGeometry =
                new EllipseGeometry(new Point(widthOffset, heightOffset), widthOffset, heightOffset);
            dc.PushClip(circleGeometry);
        }

        for (var x = 0; x < renderWidth; x++)
        {
            for (var y = 0; y < renderHeight; y++)
            {
                // 使用双线性插值计算当前点的温度
                var temperature = BilinearInterpolation(x, y, p00, p10, p01, p11);

                // 将温度映射到颜色
                var color = getColorFromTemperature(temperature);

                // 绘制像素
                dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(x, y, 1.5, 1.5));
            }
        }

        RenderPoints();
        return;

        void RenderPoints()
        {
            DrawingPointsVisual.Children.Clear();
            using var dp = DrawingPointsVisual.RenderOpen();
            var radiusOnX = renderWidth / 100;
            var radiusOnY = renderHeight / 100;
            foreach (var temperaturePoint in TemperaturePoints!)
            {
                var x = temperaturePoint.X / maxPositionOnX * renderWidth;
                var y = temperaturePoint.Y / maxPositionOnY * renderHeight;
                dp.DrawEllipse(new SolidColorBrush(Colors.Black), new Pen(Brushes.Black, 1), new Point(x, y), radiusOnX,
                    radiusOnY);
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

    protected override int VisualChildrenCount => _visualCollection.Count;

    protected override Visual GetVisualChild(int index)
    {
        return _visualCollection[index];
    }


    public void ToggleDisplayPoints()
    {
        if (TemperaturePoints == null)
        {
            return;
        }

        if (_visualCollection.Contains(DrawingPointsVisual))
        {
            _visualCollection.RemoveAt(2);
            return;
        }

        _visualCollection.Add(DrawingPointsVisual);
    }
}