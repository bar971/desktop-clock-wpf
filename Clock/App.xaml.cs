using System.Windows;

namespace Clock;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Impostazioni correnti. Null fino a OnStartup; dopo OnStartup è null
    /// solo se non esiste stato salvato (primo avvio).
    /// </summary>
    public static ClockSettings? Settings { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Settings = ClockSettings.Load();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            Settings?.Save();
        }
        catch (Exception)
        {
            // Impossibile salvare: si esce comunque senza bloccare la chiusura.
        }
        base.OnExit(e);
    }
}
