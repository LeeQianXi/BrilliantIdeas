using System;
using System.Threading.Tasks;
using Avalonia;
using DIAbstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ToDoList.Core;
using ToDoList.Core.Views;
using ToDoList.DataBase;

namespace ToDoList;

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
            .UseAvaloniaCore<ToDoListWindow>()
            .UseToDoListCore()
            .UseToDoListDbCore()
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
        return AppBuilder.Configure(() => Host.Services.GetRequiredService<ToDoListApp>())
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}