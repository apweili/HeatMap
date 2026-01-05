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

public class HeatMap : FrameworkElement
{
    static HeatMap()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HeatMap),
            new FrameworkPropertyMetadata(typeof(HeatMap)));
    }

    private const int DefaultWidth = 450;
    private const int DefaultHeight = 450;

    private DrawingVisual DrawingHeatMapVisual { get; } = new();

    protected override Size MeasureOverride(Size availableSize)
    {
        var desiredWidth = double.IsPositiveInfinity(availableSize.Width) ? DefaultWidth : availableSize.Width;
        var desiredHeight = double.IsPositiveInfinity(availableSize.Height) ? DefaultHeight : availableSize.Height;
        return new Size(desiredWidth, desiredHeight);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        RenderHeatMap(ActualWidth, ActualHeight);
    }

    private void RenderHeatMap(double width, double height)
    {
        // 定义四个点的坐标和温度值
        var p00 = new TemperaturePoint { X = 0, Y = 0, Temperature = 10 };
        var p10 = new TemperaturePoint { X = width, Y = 0, Temperature = 30 };
        var p01 = new TemperaturePoint { X = 0, Y = height, Temperature = 20 };
        var p11 = new TemperaturePoint { X = width, Y = height, Temperature = 40 };

        // 创建画笔
        var brush = new DrawingVisual();
        using (var dc = brush.RenderOpen())
        {
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
        DrawingHeatMapVisual.Children.Add(brush);
    }
    
    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index)
    {
        return DrawingHeatMapVisual!;
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
}