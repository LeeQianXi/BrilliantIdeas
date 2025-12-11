using System.Text;
using Avalonia;
using Avalonia.ReactiveUI;
using MultiPanel.Client;

namespace MultiPanel.Avalonia;

public class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        StartUp.ClientHost.StartAsync();
        Console.OutputEncoding = Encoding.UTF8;
        return BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        ServiceLocator.Instance.ClientHost = StartUp.ClientHost;
        return AppBuilder.Configure<MultiPanelApp>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}