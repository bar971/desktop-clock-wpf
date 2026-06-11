using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Clock;

public class ColorPickerDialog : CommonDialog
{
    private const double DialogWidth = 263;
    private const double DialogHeight = 346.96;

    private Color? _selectedColor;
    private readonly double _ownerTop;
    private readonly double _ownerLeft;
    private readonly double _ownerHeight;
    private readonly double _ownerWidth;
    private DockPanel _colorPickerDockPanel = null!;   // inizializzati da Reset()
    private Window _colorPickerWindow = null!;

    public ColorPickerDialog(Color selectedColor, double ownerTop, double ownerLeft, double ownerHeight, double ownerWidth)
    {
        Title = "Choose Color";
        _selectedColor = selectedColor;
        _ownerTop = ownerTop;
        _ownerLeft = ownerLeft;
        _ownerHeight = ownerHeight;
        _ownerWidth = ownerWidth;
        Reset();
    }

    private string Title { get; set; }

    public Color SelectedColor => _selectedColor ?? Colors.Transparent;

    public sealed override void Reset()
    {
        _colorPickerDockPanel = CreateColorPickerDockPanel();
        _colorPickerWindow = CreateColorPickerWindow();
    }

    protected override bool RunDialog(IntPtr hwndOwner)
    {
        ClampToVirtualScreen(_colorPickerWindow);

        _colorPickerWindow.Title = Title;

        if (_colorPickerWindow.ShowDialog() == true)
        {
            _selectedColor = _colorPickerDockPanel.Children.OfType<ColorCanvas>().First().SelectedColor;
            return true;
        }

        return false;
    }

    private void ClampToVirtualScreen(Window window)
    {
        if (window.Left < SystemParameters.VirtualScreenLeft)
            window.Left = SystemParameters.VirtualScreenLeft + _ownerHeight + 10;

        if (window.Left + DialogWidth > SystemParameters.VirtualScreenWidth)
            window.Left = SystemParameters.VirtualScreenWidth - DialogWidth - _ownerWidth - 10;

        if (window.Top < SystemParameters.VirtualScreenTop)
            window.Top = SystemParameters.VirtualScreenTop;

        if (window.Top + DialogHeight > SystemParameters.VirtualScreenHeight)
            window.Top = SystemParameters.VirtualScreenHeight - (DialogHeight + 10);
    }

    private DockPanel CreateColorPickerDockPanel()
    {
        var dockPanel = new DockPanel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(4)
        };

        var canvas = new ColorCanvas { SelectedColor = SelectedColor, Margin = new Thickness(4) };
        canvas.SetValue(DockPanel.DockProperty, Dock.Top);
        dockPanel.Children.Add(canvas);

        var okButton = new Button
        {
            IsDefault = true,
            Padding = new Thickness(16, 4, 16, 4),
            Margin = new Thickness(4),
            Content = "Ok",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        okButton.Click += (_, _) => _colorPickerWindow.DialogResult = true;
        dockPanel.Children.Add(okButton);

        return dockPanel;
    }

    private Window CreateColorPickerWindow()
    {
        return new Window
        {
            WindowStyle = WindowStyle.ToolWindow,
            Title = Title,
            Content = _colorPickerDockPanel,
            ResizeMode = ResizeMode.NoResize,
            SizeToContent = SizeToContent.WidthAndHeight,
            Top = _ownerTop + _ownerHeight + 10,
            Left = _ownerLeft + _ownerWidth + 10
        };
    }
}
