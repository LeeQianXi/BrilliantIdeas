using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiPanel.Shared.Utils;
using MultiPanel.Silo.Extensions;
using Orleans.Configuration;
using StackExchange.Redis;

namespace MultiPanel.Silo;

public static class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
            .UseOrleans(ConfigureOrleans)
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(logging => logging.AddConsole())
            .Build();

        await host.RunAsync();
    }

    private static void ConfigureOrleans(HostBuilderContext context, ISiloBuilder builder)
    {
        var configuration = context.Configuration;

        var redisConnectionString = configuration.SafeGetConfigureValue<string>("REDIS_CONNECTION_STRING");
        var redisConfig = ConfigurationOptions.Parse(redisConnectionString);
        var mysqlConnectionString = configuration.SafeGetConfigureValue<string>("MYSQL_CONNECTION_STRING");
        builder
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = configuration.SafeGetConfigureValue<string>("CLUSTER_ID");
                options.ServiceId = configuration.SafeGetConfigureValue<string>("SERVICE_ID");
            })
            .ConfigureEndpoints(
                configuration.SafeGetConfigureValue<string>("ADVERTISED_HOST"),
                configuration.SafeGetConfigureValue("SILO_PORT", 11_111),
                configuration.SafeGetConfigureValue("GATEWAY_PORT", 30_000),
                listenOnAnyHostAddress: true)
            .UseRedisClustering(options => options.ConfigurationOptions = redisConfig)
            .AddRedisGrainStorageAsDefault(options => options.ConfigurationOptions = redisConfig)
            .AddAdoNetGrainStorage("MySqlStorage", options =>
            {
                options.Invariant = configuration.SafeGetConfigureValue<string>("Orleans:AdoNetInvariant");
                options.ConnectionString = mysqlConnectionString;
            })
            .Configure<GrainCollectionOptions>(options =>
            {
                options.CollectionAge = TimeSpan.FromMinutes(10);
                options.CollectionQuantum = TimeSpan.FromMinutes(1);
            })
            .Configure<GrainDirectoryOptions>(options =>
            {
                options.CachingStrategy = GrainDirectoryOptions.CachingStrategyType.LRU;
            });
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        var redisConnectionString = context.Configuration.SafeGetConfigureValue<string>("REDIS_CONNECTION_STRING");
        collection
            .UseSiloExtension(context.Configuration)
            .AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConnectionString))
            .AddLogging(logging =>
            {
                logging.AddConsole()
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Information);
            });
    }
}