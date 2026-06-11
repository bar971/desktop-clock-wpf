using System.IO;
using System.Text.Json;

namespace Clock;

/// <summary>
/// Stato persistente dell'applicazione, serializzato in JSON in
/// %LocalAppData%\Clock\settings.json. System.Text.Json scrive i double
/// in formato invariant: nessun problema di culture.
/// </summary>
public class ClockSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Clock", "settings.json");

    public double Top { get; set; }
    public double Left { get; set; }
    public double FontSize { get; set; }
    public double Rotation { get; set; }

    /// <summary>Brush del quadrante serializzato in XAML.</summary>
    public string? ForegroundXaml { get; set; }

    /// <summary>
    /// Carica le impostazioni. Restituisce null se il file non esiste
    /// o non è leggibile (primo avvio o file corrotto).
    /// </summary>
    public static ClockSettings? Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return null;

            return JsonSerializer.Deserialize<ClockSettings>(File.ReadAllText(FilePath));
        }
        catch (Exception)
        {
            // File corrotto o inaccessibile: si riparte dai default.
            return null;
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath,
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
