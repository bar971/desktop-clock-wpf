# Clock

A small, frameless desktop clock for Windows 10 and 11. It floats on top of your
desktop showing the time in `HH:mm:ss`, and can be moved, resized, rotated and
recoloured. All settings are remembered between sessions.

Built with WPF on .NET 8.

<!-- Replace with a real screenshot of the clock on your desktop -->
<!-- ![Clock on the desktop](docs/screenshot.png) -->

## Features

- Always-on-top, borderless, transparent window
- Drag to move, mouse wheel to resize, double-click for a calendar
- Solid colours, random gradients, and animated colour effects
- Free rotation in 1° steps
- Settings persisted to a readable JSON file

## Requirements

- **To run:** the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
  (only needed for the framework-dependent build; the self-contained build bundles it).
- **To build:** the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

## Build and run

From the project folder (the one containing `Clock.csproj`):

```powershell
dotnet run
```

The first run restores the [Extended.Wpf.Toolkit](https://www.nuget.org/packages/DotNetProjects.Extended.Wpf.Toolkit)
NuGet package automatically.

To produce a standalone executable that runs on any Windows 10/11 machine without
installing .NET:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The result is a single `Clock.exe` in `bin/Release/net8.0-windows/win-x64/publish/`.

> Do not enable `PublishTrimmed`: WPF does not support trimming and the app would
> break at runtime.

## Controls

### Mouse

| Action            | Effect                                  |
|-------------------|-----------------------------------------|
| Drag (left)       | Move the clock                          |
| Wheel up / down   | Increase / decrease size                |
| Double-click      | Open the calendar                       |
| Right-click       | Quit (settings are saved)               |
| Hover ~2 s        | Show today's date as a tooltip          |

### Keyboard

| Key     | Action                                          |
|---------|-------------------------------------------------|
| `H`     | Open the help window                            |
| `+` `-` | Increase / decrease size (numpad works too)     |
| `I`     | Rotate clockwise by 1°                          |
| `P`     | Rotate counter-clockwise by 1°                  |
| `T`     | Toggle "always on top"                          |
| `C`     | Open the colour picker                          |
| `B`     | Random solid colour                             |
| `R`     | Random colour gradient                          |
| `A`     | Start / pause the full colour animation         |
| `S`     | Start / pause the simple animation              |
| `O`     | Restore the original colour                     |

## Settings

Position, size, rotation and colour are saved on exit to:

```
%LocalAppData%\Clock\settings.json
```

This is a plain-text JSON file you can open with any text editor.

### Recovering an off-screen clock

If the clock ends up beyond the edge of your screen (for example after
disconnecting a second monitor), close it, open `settings.json`, set
`"Top": 100` and `"Left": 100`, save, and restart. Deleting the file resets
all settings to their defaults.

## License

Released under the [MIT License](LICENSE).
