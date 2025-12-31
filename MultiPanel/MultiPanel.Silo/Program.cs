using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        var redisConnectionString = SafeGetConfigureValue<string>(configuration, "REDIS_CONNECTION_STRING");
        var redisConfig = ConfigurationOptions.Parse(redisConnectionString);
        var mysqlConnectionString = SafeGetConfigureValue<string>(configuration, "MYSQL_CONNECTION_STRING");
        builder
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = SafeGetConfigureValue<string>(configuration, "CLUSTER_ID");
                options.ServiceId = SafeGetConfigureValue<string>(configuration, "SERVICE_ID");
            })
            .ConfigureEndpoints(
                SafeGetConfigureValue<string>(configuration, "ADVERTISED_HOST"),
                SafeGetConfigureValue(configuration, "SILO_PORT", 11_111),
                SafeGetConfigureValue(configuration, "GATEWAY_PORT", 30_000),
                listenOnAnyHostAddress: true)
            .UseRedisClustering(options => options.ConfigurationOptions = redisConfig)
            .AddRedisGrainStorageAsDefault(options => options.ConfigurationOptions = redisConfig)
            .AddAdoNetGrainStorage("MySqlStorage", options =>
            {
                options.Invariant = SafeGetConfigureValue<string>(configuration, "Orleans:AdoNetInvariant");
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
        var redisConnectionString = SafeGetConfigureValue<string>(context.Configuration, "REDIS_CONNECTION_STRING");
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

    private static T SafeGetConfigureValue<T>(IConfiguration configuration, string key)
    {
        return configuration.GetValue<T>(key) ??
               throw new KeyNotFoundException($"\"{key}\" is a required configure key");
    }

    private static T SafeGetConfigureValue<T>(IConfiguration configuration, string key, T defaultValue)
    {
        return configuration.GetValue<T>(key) ?? defaultValue;
    }
}