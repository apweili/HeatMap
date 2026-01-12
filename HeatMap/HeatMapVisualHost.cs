using System.Diagnostics.CodeAnalysis;
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

    public void SetHeatMap(HeatMapSetting heatMapSetting, IEnumerable<TemperaturePoint> temperaturePoints)
    {
        HeatMapSetting = heatMapSetting;
        TemperaturePoints = temperaturePoints?.Select(t => new TemperaturePoint
        {
            X = t.X + HeatMapSetting.MaxHorizontalPosition / 2,
            Y = t.Y + HeatMapSetting.MaxVerticalPosition / 2,
            Z = t.Z,
            Temperature = t.Temperature
        }).ToList();
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
        NormalDistribution.SetStandard(HeatMapSetting, renderWidth, renderHeight);
        var normalDistributions = TemperaturePoints!
            .Select(p => new NormalDistribution(p, HeatMapSetting, renderWidth, renderHeight)).ToArray();
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
                var temperature = EstimateTemperature(normalDistributions, x, y);
                var color = getColorFromTemperature(temperature);
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

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private static double EstimateTemperature(IEnumerable<NormalDistribution> normalDistribution, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(normalDistribution);
        var candidates = normalDistribution.Select(n =>
            new
            {
                Formular = n,
                Distance = Math.Sqrt(Math.Pow(n.X - x, 2) + Math.Pow(n.Y - y, 2))
            }
        ).Select(n =>
            new
            {
                TemperatureLimit = n.Formular.Temperature,
                Temperature = n.Formular.EstimateTemperature(n.Distance),
                Distance = n.Distance
            }).ToList();

        if (candidates.Count == 1)
        {
            return candidates[0].Temperature;
        }

        var temperaturesUpZero = candidates.Where(o => o.Temperature >= 0)
            .OrderByDescending(c => c.Temperature);
        var result1 = 0d;
        int i = 1;
        var maxTemperatureObject = temperaturesUpZero.FirstOrDefault();
        foreach (var candidate in temperaturesUpZero.Select(t => t.Temperature))
        {
            result1 += Math.Abs(candidate - result1) / (i * i);
            if (result1 >= maxTemperatureObject!.TemperatureLimit)
            {
                result1 = maxTemperatureObject.TemperatureLimit;
                break;
            }
            i++;
        }

        var temperaturesBelowZero = candidates.Where(o => o.Temperature < 0)
            .OrderBy(c => c.Temperature);
        var minTemperatureObject = temperaturesBelowZero.FirstOrDefault();
        i = 1;
        var result2 = 0d;
        foreach (var candidate in temperaturesBelowZero.Select(t => t.Temperature))
        {
            result2 -= Math.Abs(candidate - result1) / (i * i);
            if (result2 <= minTemperatureObject!.TemperatureLimit)
            {
                result2 = minTemperatureObject.TemperatureLimit;
                break;
            }

            i++;
        }

        var result = result1 + result2;
        if (result1 == 0 || result2 == 0)
        {
            return result;
        }

        var maxTemperatureDistance = maxTemperatureObject!.Distance;
        var minTemperatureDistance = minTemperatureObject!.Distance;
        return (result1 * minTemperatureDistance * minTemperatureDistance +
                result2 * maxTemperatureDistance * maxTemperatureDistance) /
               (maxTemperatureDistance * maxTemperatureDistance + minTemperatureDistance * minTemperatureDistance);
    }

    private class NormalDistribution(
        TemperaturePoint temperaturePoints,
        HeatMapSetting setting,
        double width,
        double height)
    {
        public static double Standard { get; private set; }
        private const double StandardFactor = 80;
        public double X { get; } = temperaturePoints.X / setting.MaxHorizontalPosition * width;
        public double Y { get; } = temperaturePoints.Y / setting.MaxVerticalPosition * height;

        public double Temperature { get; } = temperaturePoints.Temperature;

        public static void SetStandard(HeatMapSetting setting, double width,
            double height)
        {
            Standard = StandardFactor * Math.Sqrt(
                Math.Pow(width / setting.MaxHorizontalPosition, 2) + Math.Pow(height /
                                                                              setting.MaxVerticalPosition, 2));
        }

        public double EstimateTemperature(double offset)
        {
            return Math.Exp(-1d / 2 * Math.Pow(offset / Standard, 2.0)) * Temperature;
        }
    }
}