using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Clock;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private const double MinFontSize = 25;
    private const double MaxFontSize = 200;
    private const double FontSizeStep = 5;

    private readonly DispatcherTimer _clockTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private readonly DispatcherTimer _borderHideTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly Brush _originalBrush;
    private readonly Random _rnd = new();
    private readonly RotateTransform _rotateTransform = new(0);

    private bool _alwaysOnTop = true;
    private Storyboard _gradientStopAnimationStoryboard = new();
    private bool _animationInProgress;
    private bool _animationInPause;
    private Key _lastAnimationKey;

    public MainWindow()
    {
        InitializeComponent();
        ClockLabel.Visibility = Visibility.Hidden;
        Topmost = _alwaysOnTop;

        _clockTimer.Tick += ClockTimer_Tick;
        _clockTimer.Start();
        _borderHideTimer.Tick += BorderHideTimer_Tick;

        _originalBrush = ClockLabel.Foreground;
        NameScope.SetNameScope(ClockLabel, new NameScope());

        ApplySavedSettings();
    }

    private void ApplySavedSettings()
    {
        ClockSettings? settings = App.Settings;
        if (settings is null)
            return; // Primo avvio: si usano i default dello XAML.

        Top = settings.Top;
        Left = settings.Left;
        ClockLabel.FontSize = settings.FontSize;
        _rotateTransform.Angle = settings.Rotation;
        ClockBorder.LayoutTransform = _rotateTransform;
        _alwaysOnTop = false;
        Topmost = false;

        if (!string.IsNullOrEmpty(settings.ForegroundXaml))
        {
            try
            {
                ClockLabel.Foreground = (Brush)Deserialize(settings.ForegroundXaml);
            }
            catch (Exception)
            {
                // Brush salvato non deserializzabile: si mantiene quello di default.
            }
        }
    }

    /// <summary>
    /// Fotografa lo stato corrente nelle impostazioni. Chiamato una sola
    /// volta alla chiusura: App.OnExit si occupa del salvataggio su disco.
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        App.Settings = new ClockSettings
        {
            Top = Top,
            Left = Left,
            FontSize = ClockLabel.FontSize,
            Rotation = ClockBorder.LayoutTransform is RotateTransform rotate ? rotate.Angle % 360 : 0,
            ForegroundXaml = Serialize(ClockLabel.Foreground)
        };

        base.OnClosing(e);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private string? _currentDateAndTime;
    public string? CurrentDateAndTime
    {
        get => _currentDateAndTime;
        private set
        {
            if (_currentDateAndTime == value)
                return;
            _currentDateAndTime = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentDateAndTime)));
        }
    }

    private void ClockTimer_Tick(object? sender, EventArgs e)
    {
        CurrentDateAndTime = $" {DateTime.Now:HH:mm:ss} ";
        if (ClockLabel.Visibility != Visibility.Visible)
            ClockLabel.Visibility = Visibility.Visible;
    }

    private void ClockLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        DragMove();
    }

    private void ClockLabel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        _clockTimer.Stop();
        Close(); // OnClosing fotografa lo stato, App.OnExit lo salva.
    }

    private void ClockLabel_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;
        if (e.Delta > 0)
            SizeUp();
        else
            SizeDown();
    }

    private void SizeUp()
    {
        if (ClockLabel.FontSize >= MaxFontSize)
            return;
        ClockLabel.FontSize += FontSizeStep;
        ClockBorder.Height = ClockLabel.ActualHeight + 25;
        ClockBorder.Width = ClockLabel.ActualWidth + 25;
    }

    private void SizeDown()
    {
        if (ClockLabel.FontSize <= MinFontSize)
            return;
        ClockLabel.FontSize -= FontSizeStep;
        ClockBorder.Height = ClockLabel.ActualHeight;
        ClockBorder.Width = ClockLabel.ActualWidth - 10;
    }

    private void ClockLabel_MouseMove(object sender, MouseEventArgs e)
    {
        TodayDate.Background = ClockLabel.Foreground;
        TodayDate.Content = DateTime.Today.ToLongDateString();
    }

    private void TodayDate_Opened(object sender, RoutedEventArgs e)
    {
        if (!_alwaysOnTop) ClockWindow.Topmost = true;
    }

    private void TodayDate_Closed(object sender, RoutedEventArgs e)
    {
        if (!_alwaysOnTop) ClockWindow.Topmost = false;
    }

    private void ClockLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var cal = new Calendar();
        PositionAndShow(cal, cal.CalendarBorder, 173, 185, asDialog: false);
    }

    private void ClockLabel_MouseEnter(object sender, MouseEventArgs e)
    {
        _borderHideTimer.Start();
        ClockBorder.CornerRadius = new CornerRadius(20);
        ClockBorder.BorderThickness = new Thickness(1);
        ClockBorder.BorderBrush = ClockLabel.Foreground;
    }

    private void BorderHideTimer_Tick(object? sender, EventArgs e)
    {
        _borderHideTimer.Stop();
        ClockBorder.BorderThickness = new Thickness(0);
    }

    /// <summary>
    /// Posiziona una finestra secondaria accanto all'orologio, vincolata
    /// allo schermo virtuale, e la mostra.
    /// </summary>
    private void PositionAndShow(Window window, Border accentBorder, double height, double width, bool asDialog)
    {
        window.Owner = this;
        accentBorder.BorderBrush = ClockLabel.Foreground;

        window.Top = Top + ClockBorder.ActualHeight + 5;
        window.Left = Left + ClockBorder.ActualWidth + 5;

        if (window.Left < SystemParameters.VirtualScreenLeft)
            window.Left = SystemParameters.VirtualScreenLeft + ClockBorder.ActualWidth + 5;

        if (window.Left + width > SystemParameters.VirtualScreenWidth)
            window.Left = SystemParameters.VirtualScreenWidth - width - ClockBorder.ActualWidth - 5;

        if (window.Top < SystemParameters.VirtualScreenTop)
            window.Top = SystemParameters.VirtualScreenTop;

        if (window.Top + height > SystemParameters.VirtualScreenHeight)
            window.Top = SystemParameters.VirtualScreenHeight - (height + 10) - ClockBorder.ActualHeight - 5;

        if (asDialog)
            window.ShowDialog();
        else
            window.Show();
    }

    private LinearGradientBrush RandomGradient()
    {
        UnregisterGradientStopNames();

        var brush = new LinearGradientBrush();
        double[] offsets = [0, 0.33, 0.66, 1];

        for (int i = 0; i < offsets.Length; i++)
        {
            var stop = new GradientStop(RndColor(), offsets[i]);
            ClockLabel.RegisterName($"GS{i + 1}", stop);
            brush.GradientStops.Add(stop);
        }

        return brush;
    }

    private Color RndColor() =>
        Color.FromRgb((byte)_rnd.Next(256), (byte)_rnd.Next(256), (byte)_rnd.Next(256));

    private static bool IsNameRegistered(DependencyObject depObject, string name) =>
        NameScope.GetNameScope(depObject)?.FindName(name) is not null;

    private void UnregisterGradientStopNames()
    {
        string[] names = ["GS1", "GS2", "GS3", "GS4"];
        foreach (string name in names)
        {
            if (IsNameRegistered(ClockLabel, name))
                ClockLabel.UnregisterName(name);
        }
    }

    private void StopAnimation()
    {
        if (!_animationInProgress)
            return;
        _gradientStopAnimationStoryboard.Stop(ClockLabel);
        _gradientStopAnimationStoryboard.Seek(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
        _animationInProgress = false;
    }
}
