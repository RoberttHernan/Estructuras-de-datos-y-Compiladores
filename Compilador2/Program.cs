using Avalonia;
using Avalonia.ReactiveUI;

namespace Compilador2;

// Punto de entrada de la aplicacion Avalonia.
// Delega inmediatamente al builder de Avalonia con deteccion automatica de plataforma
// y soporte para ReactiveUI (MVVM).
class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                     .UsePlatformDetect()
                     .LogToTrace()
                     .UseReactiveUI();
}
