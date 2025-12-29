using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultiPanel.Client;
using MultiPanel.Client.Abstract.Options;
using MultiPanel.Client.Orleans;
using MultiPanel.Client.Services;
using MultiPanel.Client.Views;
using NetUtility.Singleton;
using Orleans.Configuration;

namespace MultiPanel.Avalonia;

public class ClientContext : StaticSingleton<ClientContext>, IClientContext
{
    private bool _isInitialized;

    public ClientContext()
    {
        if (Instance is not null)
            throw new InvalidOperationException("ClientContext already initialized");
        ClientHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(OnConfigureAppConfiguration)
            .ConfigureServices(OnConfigureServices)
            .UseOrleansClient(OnConfigureClient)
            .UseConsoleLifetime()
            .Build();
    }

    [field: AllowNull]
    public IClusterClient Client =>
        field ??= _isInitialized && IsConnected
            ? ServiceProvider.GetRequiredService<IClusterClient>()
            : throw new Exception("Client not initialized");

    public IHost ClientHost { get; }
    public IServiceProvider ServiceProvider => ClientHost.Services;

    public async Task<bool> TryConnect()
    {
        if (_isInitialized)
            return IsConnected;
        _isInitialized = true;
        try
        {
            await ClientHost.StartAsync();
            IsConnected = true;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsConnected { get; private set; }

    private static void OnConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder cbuilder)
    {
        cbuilder
            .AddJsonFile("appsettings.json", false, true);
    }

    private static void OnConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection
            .Configure<LoginWithOptions>(context.Configuration)
            .AddSingleton<IClientContext, ClientContext>(_ => Instance)
            .UseAvaloniaCore<LoginInWindow>()
            .UseMultiPanelClient(context.Configuration);
    }

    private static void OnConfigureClient(HostBuilderContext context, IClientBuilder client)
    {
        var configuration = context.Configuration;
        var redisConnectionString = configuration.GetConnectionString("Redis");
        client
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = configuration["Orleans:ClusterId"] ?? "dev";
                options.ServiceId = configuration["Orleans:ServiceId"] ?? "OrleansAuthentication";
            })
            .UseRedisClustering(redisConnectionString)
            .UseConnectionRetryFilter<CustomClientConnectionRetryFilter>();
    }
}