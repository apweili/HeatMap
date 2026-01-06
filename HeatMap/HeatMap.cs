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

[TemplatePart(Name = HeatMapVisualHostTemplateName, Type = typeof(Canvas))]
public class HeatMap : Control
{
    static HeatMap()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HeatMap),
            new FrameworkPropertyMetadata(typeof(HeatMap)));
        SnapsToDevicePixelsProperty.OverrideMetadata(typeof(HeatMap), new FrameworkPropertyMetadata(true));
    }

    private const string HeatMapVisualHostTemplateName = "PART_HeatMapVisualHost";

    private HeatMapVisualHost? HeatMapVisualHost { get; set; }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HeatMapVisualHost = (HeatMapVisualHost)GetTemplateChild(HeatMapVisualHostTemplateName)!;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        HeatMapVisualHost!.Measure(new Size(constraint.Width / 5 * 4, constraint.Height));
        return new Size(HeatMapVisualHost.DesiredSize.Width + HeatMapVisualHost.DesiredSize.Width / 4,
            HeatMapVisualHost.DesiredSize.Height);
    }
}