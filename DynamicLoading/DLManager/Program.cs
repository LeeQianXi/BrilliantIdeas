using Avalonia;
using Avalonia.ReactiveUI;
using DIAbstract;
using DLManager.Core;
using DLManager.Core.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DLManager;

internal static class Program
{
    private static readonly IHost Host;

    static Program()
    {
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((Action<HostBuilderContext, IServiceCollection>)ConfigureServices)
            .Build();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        var config = context.Configuration;
        collection
            .UseAvaloniaCore<DlManagerWindow>()
            .UseDlManagerCore()
            .AddDlPlugin()
            .UseBrilliantInitializer();
    }

    [STAThread]
    public static async Task Main(string[] args)
    {
        await Host.StartAsync();
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        await Host.StopAsync();
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure(() => Host.Services.GetRequiredService<DlManagerApp>())
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}