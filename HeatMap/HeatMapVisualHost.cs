using System.Windows;
using System.Windows.Media;

namespace CylindricalTopSurface;

public class HeatMapVisualHost : UIElement
{
    private const int DefaultWidth = 400;
    private const int DefaultHeight = 400;
    private DrawingVisual DrawingHeatMapVisual { get; } = new();

    protected override Size MeasureCore(Size availableSize)
    {
        if (double.IsPositiveInfinity(availableSize.Width))
        {
            availableSize.Width = DefaultWidth;
        }

        if (double.IsPositiveInfinity(availableSize.Height))
        {
            availableSize.Height = DefaultHeight;
        }

        return availableSize;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo info)
    {
        base.OnRenderSizeChanged(info);
        var newSize = info.NewSize;
        RenderHeatMap(newSize.Width, newSize.Height);
    }

    private void RenderHeatMap(double width, double height)
    {
        // 定义四个点的坐标和温度值
        var p00 = new TemperaturePoint { X = 0, Y = 0, Temperature = 10 };
        var p10 = new TemperaturePoint { X = width, Y = 0, Temperature = 30 };
        var p01 = new TemperaturePoint { X = 0, Y = height, Temperature = 20 };
        var p11 = new TemperaturePoint { X = width, Y = height, Temperature = 40 };

        var heatMapVisual = new DrawingVisual();
        using (var dc = heatMapVisual.RenderOpen())
        {
            var circleRadius = width / 2;
            // 定义圆形几何
            var circleGeometry = new EllipseGeometry(new Point(circleRadius, circleRadius), circleRadius, circleRadius);

            // 应用裁剪路径
            dc.PushClip(circleGeometry);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // 使用双线性插值计算当前点的温度
                    double temperature = BilinearInterpolation(x, y, p00, p10, p01, p11);

                    // 将温度映射到颜色
                    Color color = GetColorFromTemperature(temperature);

                    // 绘制像素
                    dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(x, y, 1.5, 1.5));
                }
            }
        }

        DrawingHeatMapVisual.Children.Clear();
        DrawingHeatMapVisual.Children.Add(heatMapVisual);
    }

    private static double BilinearInterpolation(double x, double y, TemperaturePoint p00, TemperaturePoint p10,
        TemperaturePoint p01, TemperaturePoint p11)
    {
        double t = (x - p00.X) / (p10.X - p00.X);
        double a = p00.Temperature * (1 - t) + p10.Temperature * t;
        double b = p01.Temperature * (1 - t) + p11.Temperature * t;

        double u = (y - p00.Y) / (p01.Y - p00.Y);
        return a * (1 - u) + b * u;
    }

    private static Color GetColorFromTemperature(double temperature)
    {
        // 假设温度范围是10到40
        const double minTemp = 10;
        const double maxTemp = 40;

        // 将温度映射到0-1之间
        double normalizedTemp = (temperature - minTemp) / (maxTemp - minTemp);

        // 使用蓝色到红色的渐变
        byte red = (byte)(255 * normalizedTemp);
        byte green = 0;
        byte blue = (byte)(255 * (1 - normalizedTemp));

        return Color.FromRgb(red, green, blue);
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index)
    {
        return DrawingHeatMapVisual!;
    }
}