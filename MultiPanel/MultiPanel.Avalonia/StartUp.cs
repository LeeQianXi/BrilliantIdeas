using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultiPanel.Client;
using MultiPanel.Client.Services;
using MultiPanel.Client.Views;
using Orleans.Configuration;

namespace MultiPanel.Avalonia;

public static class StartUp
{
    static StartUp()
    {
        ClientHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, cbuilder) => { cbuilder.UseDlManagerOptions(); })
            .ConfigureServices(services => { services.StartUpWith(); })
            .UseOrleansClient(client =>
            {
                client.UseLocalhostClustering()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "multi-panel-cluster";
                        options.ServiceId = "MultiPanelSilo";
                    })
                    .UseConnectionRetryFilter<CustomClientConnectionRetryFilter>();
            })
            .UseConsoleLifetime()
            .Build();
    }

    public static IHost ClientHost { get; }

    private static IServiceCollection StartUpWith(this IServiceCollection collection)
    {
        return collection
            .UseAvaloniaCore<StartUpLoadingWindow>();
    }

    private static IConfigurationBuilder UseDlManagerOptions(this IConfigurationBuilder builder)
    {
        return builder
            .AddJsonFile("appsettings.json", false, true);
    }
}