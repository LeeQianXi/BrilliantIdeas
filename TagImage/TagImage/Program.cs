using System;
using System.Threading.Tasks;
using Avalonia;
using DIAbstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TagImage.Core;
using TagImage.Core.Views;
using TagImage.Database;

namespace TagImage;

public class Program
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
            .UseAvaloniaCore<TagImageWindow>()
            .UseTagImageCore()
            .UseTagImageDbCore()
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
        return AppBuilder.Configure(() => Host.Services.GetRequiredService<TagImageApp>())
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}