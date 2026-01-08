using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HeatMap;

public partial class HeatMapControl
{
    private Lazy<Popup> Popup { get; } = new(PopupFactory);

    private static Popup PopupFactory()
    {
        var popup = new Popup
        {
            Child = new TextBlock
            {
                Focusable = true,
                Background = Brushes.White,
                Foreground = Brushes.Black
            }
        };

        return popup;
    }

    private void AddHandlersForPopup()
    {
        PreviewMouseDown -= OnPreviewMouseDown;
        PreviewMouseDown += OnPreviewMouseDown;
        AddHandlersForHeatMapVisualHost();
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        EnsurePopupClosed();
    }

    private void AddHandlersForHeatMapVisualHost()
    {
        HeatMapVisualHost!.RemoveHandler(MouseRightButtonUpEvent,
            new MouseButtonEventHandler(HandleMouseRightButtonUpOnHeatMapVisualHost));
        HeatMapVisualHost.AddHandler(MouseRightButtonUpEvent,
            new MouseButtonEventHandler(HandleMouseRightButtonUpOnHeatMapVisualHost));
    }

    private void HandleMouseRightButtonUpOnHeatMapVisualHost(object sender, MouseButtonEventArgs e)
    {
        var heatMapVisualHost = (HeatMapVisualHost)sender;
        var point = e.GetPosition(heatMapVisualHost);
        var background = (Rectangle)VisualTreeHelper.GetChild(heatMapVisualHost, 0);
        if (Equals(e.Source, background))
        {
            return;
        }

        var horizonPosition = Math.Round(point.X / background.ActualWidth * MaxHorizontalPosition, 2);
        var verticalPosition = Math.Round(point.Y / background.ActualHeight * MaxVerticalPosition, 2);
        var popup = Popup.Value;
        ((TextBlock)popup.Child).Text = $"x:{horizonPosition}, y:{verticalPosition}";
        popup.Placement = PlacementMode.MousePoint;
        popup.PlacementTarget = this;
        popup.IsOpen = true;
    }

    private void EnsurePopupClosed()
    {
        if (Popup is { IsValueCreated: true, Value.IsOpen: true })
            Popup.Value.IsOpen = false;
    }
}