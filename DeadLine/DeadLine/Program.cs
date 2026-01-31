using Avalonia;
using Avalonia.ReactiveUI;
using DeadLine.Core;
using DeadLine.Core.Views;
using DeadLine.DataBase.Core;
using DIAbstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeadLine;

internal static class Program
{
    private static readonly IHost Host;

    static Program()
    {
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        var config = context.Configuration;
        collection
            .UseAvaloniaCore<DeadLineWindow>()
            .UseDeadLineCore()
            .UseDeadLineDataBase()
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
        return AppBuilder.Configure(() => Host.Services.GetRequiredService<DeadLineApp>())
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}