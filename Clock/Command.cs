using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Clock;

public partial class MainWindow
{
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        switch (e.Key)
        {
            case Key.A:
            case Key.S:
                ToggleGradientAnimation(e.Key);
                break;

            case Key.C:
                PickColor();
                break;

            case Key.B:
                StopAnimation();
                ClockLabel.Foreground = new SolidColorBrush(RndColor());
                break;

            case Key.O:
                StopAnimation();
                UnregisterGradientStopNames();
                ClockLabel.Foreground = _originalBrush;
                break;

            case Key.R:
                StopAnimation();
                LinearGradientBrush gradient = RandomGradient();
                gradient.StartPoint = new Point(0, 0.5);
                gradient.EndPoint = new Point(1, 0.5);
                ClockLabel.Foreground = gradient;
                break;

            case Key.OemPlus:
            case Key.Add:
                SizeUp();
                break;

            case Key.OemMinus:
            case Key.Subtract:
                SizeDown();
                break;

            case Key.I:
                Rotate(+1);
                break;

            case Key.P:
                Rotate(-1);
                break;

            case Key.T:
                _alwaysOnTop = !_alwaysOnTop;
                Topmost = _alwaysOnTop;
                break;

            case Key.H:
                var help = new Help();
                PositionAndShow(help, help.HelpBorder, 266, 298, asDialog: true);
                break;
        }
    }

    private void Rotate(double delta)
    {
        double angle = _rotateTransform.Angle + delta;
        if (angle >= 360) angle -= 360;
        if (angle < 0) angle += 360;
        _rotateTransform.Angle = angle;
        ClockBorder.LayoutTransform = _rotateTransform;
    }

    private void PickColor()
    {
        StopAnimation();

        Brush originalBrush = ClockLabel.Foreground;
        Color sourceColor = originalBrush is SolidColorBrush solid ? solid.Color : Colors.Black;

        var dialog = new ColorPickerDialog(sourceColor, Top, Left,
            ClockBorder.ActualHeight, ClockBorder.ActualWidth);

        ClockLabel.Foreground = dialog.ShowDialog() == true
            ? new SolidColorBrush(dialog.SelectedColor)
            : originalBrush;
    }

    private void ToggleGradientAnimation(Key key)
    {
        // Cambiando tasto (A <-> S) l'animazione corrente viene azzerata.
        if (key != _lastAnimationKey)
        {
            _gradientStopAnimationStoryboard.Stop(ClockLabel);
            _animationInProgress = false;
            _animationInPause = false;
        }

        if (_animationInProgress)
        {
            _gradientStopAnimationStoryboard.Pause(ClockLabel);
            _animationInProgress = false;
            _animationInPause = true;
            return;
        }

        if (!_animationInPause)
            _gradientStopAnimationStoryboard = BuildGradientStoryboard(key);

        if (_animationInPause)
        {
            _gradientStopAnimationStoryboard.Resume(ClockLabel);
            _animationInPause = false;
        }
        else
        {
            _gradientStopAnimationStoryboard.Begin(ClockLabel, true);
        }

        _animationInProgress = true;
        _lastAnimationKey = key;
    }

    private Storyboard BuildGradientStoryboard(Key key)
    {
        EnsureGradientForeground();

        var storyboard = new Storyboard();

        // Offset del primo stop: da 0 a 1, in loop.
        var offsetAnimation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromSeconds(3),
            RepeatBehavior = RepeatBehavior.Forever
        };
        Storyboard.SetTargetName(offsetAnimation, "GS1");
        Storyboard.SetTargetProperty(offsetAnimation, new PropertyPath(GradientStop.OffsetProperty));
        storyboard.Children.Add(offsetAnimation);

        // Con A si animano anche i colori degli altri tre stop, scaglionati.
        if (key == Key.A)
        {
            storyboard.Children.Add(CreateColorAnimation("GS2", beginSeconds: 3));
            storyboard.Children.Add(CreateColorAnimation("GS3", beginSeconds: 6));
            storyboard.Children.Add(CreateColorAnimation("GS4", beginSeconds: 9));
        }

        return storyboard;
    }

    private ColorAnimation CreateColorAnimation(string targetName, double beginSeconds)
    {
        var animation = new ColorAnimation
        {
            From = RndColor(),
            To = RndColor(),
            Duration = TimeSpan.FromSeconds(5),
            RepeatBehavior = RepeatBehavior.Forever,
            BeginTime = TimeSpan.FromSeconds(beginSeconds)
        };
        Storyboard.SetTargetName(animation, targetName);
        Storyboard.SetTargetProperty(animation, new PropertyPath(GradientStop.ColorProperty));
        return animation;
    }

    /// <summary>
    /// Garantisce che il foreground sia un LinearGradientBrush con gli stop
    /// registrati nel NameScope (GS1..GS4), requisito dello Storyboard.
    /// </summary>
    private void EnsureGradientForeground()
    {
        if (ClockLabel.Foreground is LinearGradientBrush gradient)
        {
            if (ClockLabel.FindName("GS1") is null)
            {
                for (int i = 0; i < 4; i++)
                    ClockLabel.RegisterName($"GS{i + 1}", gradient.GradientStops[i]);
            }
        }
        else
        {
            ClockLabel.Foreground = RandomGradient();
        }
    }
}
