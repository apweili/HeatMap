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
using CylindricalTopSurface;

namespace WpfApp1;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int CanvasWidth = 800;
    private const int CanvasHeight = 450;

    public MainWindow()
    {
        InitializeComponent();
        canvas.Children.Add(new HeatMap());
    }

    private void RenderHeatMap()
    {
        // 定义四个点的坐标和温度值
        var p00 = new TemperaturePoint { X = 0, Y = 0, Temperature = 10 };
        var p10 = new TemperaturePoint { X = CanvasWidth, Y = 0, Temperature = 30 };
        var p01 = new TemperaturePoint { X = 0, Y = CanvasHeight, Temperature = 20 };
        var p11 = new TemperaturePoint { X = CanvasWidth, Y = CanvasHeight, Temperature = 40 };

        // 创建画笔
        var brush = new DrawingVisual();
        using (var dc = brush.RenderOpen())
        {
            for (int x = 0; x < CanvasWidth; x++)
            {
                for (int y = 0; y < CanvasHeight; y++)
                {
                    // 使用双线性插值计算当前点的温度
                    double temperature = Helpers.BilinearInterpolation(x, y, p00, p10, p01, p11);

                    // 将温度映射到颜色
                    Color color = Helpers.GetColorFromTemperature(temperature);

                    // 绘制像素
                    dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(x, y, 1, 1));
                }
            }
        }

        // 将绘制的内容添加到画布
        // canvas.Children.Add(new DrawingVisual { Visual = brush });
    }
}

public class TemperaturePoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Temperature { get; set; }
}