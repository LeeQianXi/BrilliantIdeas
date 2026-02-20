using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using GeneralEditor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal static class Program
{
    private static readonly IHost Host;

    static Program()
    {
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureAvaloniaServices()
            .Build();
    }

    [STAThread]
    private static async Task Main(string[] args)
    {
        await Host.StartAsync();
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args, ShutdownMode.OnLastWindowClose);
        await Host.StopAsync();
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure(() => Host.Services.GetRequiredService<GeneralEditorApp>())
            .UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                UseDBusMenu = false,
                UseDBusFilePicker = false
            })
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}