using System.Windows.Media;

namespace WpfApp1;

public static class Helpers
{
    public static double BilinearInterpolation(double x, double y, TemperaturePoint p00, TemperaturePoint p10,
        TemperaturePoint p01, TemperaturePoint p11)
    {
        double t = (x - p00.X) / (p10.X - p00.X);
        double a = p00.Temperature * (1 - t) + p10.Temperature * t;
        double b = p01.Temperature * (1 - t) + p11.Temperature * t;

        double u = (y - p00.Y) / (p01.Y - p00.Y);
        return a * (1 - u) + b * u;
    }
    
    public static Color GetColorFromTemperature(double temperature)
    {
        // 假设温度范围是10到40
        double minTemp = 10;
        double maxTemp = 40;

        // 将温度映射到0-1之间
        double normalizedTemp = (temperature - minTemp) / (maxTemp - minTemp);

        // 使用蓝色到红色的渐变
        byte red = (byte)(255 * normalizedTemp);
        byte green = 0;
        byte blue = (byte)(255 * (1 - normalizedTemp));

        return Color.FromRgb(red, green, blue);
    }
}