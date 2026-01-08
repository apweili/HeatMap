using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        HeatMapVisualHost!.RemoveHandler(MouseLeftButtonUpEvent,
            new MouseButtonEventHandler(HandleMouseLeftButtonUpOnHeatMapVisual));
        HeatMapVisualHost.AddHandler(MouseLeftButtonUpEvent,
            new MouseButtonEventHandler(HandleMouseLeftButtonUpOnHeatMapVisual));
    }

    private void HandleMouseLeftButtonUpOnHeatMapVisual(object sender, MouseButtonEventArgs e)
    {
        var heatMapVisualHost = (HeatMapVisualHost)sender;
        if (IsHitOnHeatMapVisualBackground(heatMapVisualHost, e, out var background))
        {
            return;
        }

        var point = e.GetPosition(heatMapVisualHost);
        var horizonPosition =
            Math.Round(point.X / background.ActualWidth * MaxHorizontalPosition - MaxHorizontalPosition / 2, 2);
        var verticalPosition =
            Math.Round(MaxVerticalPosition / 2 - point.Y / background.ActualHeight * MaxVerticalPosition, 2);
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

    private ContextMenu CreateContextMenu()
    {
        var contextMenu = new ContextMenu();

        var menuItem1 = new MenuItem { Header = "Save Image", Command = SaveImageCommand, CommandTarget = this };
        var menuItem2 = new MenuItem { Header = "Option 2" };

        contextMenu.Items.Add(menuItem1);
        contextMenu.Items.Add(menuItem2);

        contextMenu.Placement = PlacementMode.MousePoint;
        contextMenu.PlacementTarget = this;

        return contextMenu;
    }

    private void OnSaveImageExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PNG Image|*.png",
            DefaultExt = ".png"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            var renderBitmap = new RenderTargetBitmap(
                (int)RenderSize.Width,
                (int)RenderSize.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);

            renderBitmap.Render(this);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            using var stream = saveFileDialog.OpenFile();
            encoder.Save(stream);
        }
    }

    private static bool IsHitOnHeatMapVisualBackground(HeatMapVisualHost heatMapVisualHost,
        RoutedEventArgs routedEventArgs,
        out FrameworkElement background)
    {
        background = heatMapVisualHost.Background;
        return background == routedEventArgs.Source;
    }
}