using Avalonia;
using Avalonia.ReactiveUI;
using AvaloniaUtility.Services;
using DIAbstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DLManager.Plugin.Preview;

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
            .AddMultiSingleton<Application, PreviewApp>()
            .AddMultiSingleton<IStartupWindow, StartupWindow>()
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
        return AppBuilder.Configure(() => Host.Services.GetRequiredService<PreviewApp>())
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}