using System.Text;
using Avalonia;
using Avalonia.ReactiveUI;
using MultiPanel.Client;

namespace MultiPanel.Avalonia;

public class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        await StartUp.ClientHost.StartAsync();
        Console.OutputEncoding = Encoding.UTF8;
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        ServiceLocator.Instance.ServiceProvider = StartUp.ServiceProvider;
        return AppBuilder.Configure<MultiPanelApp>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}