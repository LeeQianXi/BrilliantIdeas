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

        var redisConnectionString = configuration["REDIS_CONNECTION_STRING"];
        var mysqlConnectionString = configuration["MYSQL_CONNECTION_STRING"];
        builder
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = configuration["CLUSTER_ID"];
                options.ServiceId = configuration["SERVICE_ID"];
            })
            .ConfigureEndpoints(
                configuration["ADVERTISED_HOST"],
                configuration.GetValue("SILO_PORT", 11_111),
                configuration.GetValue("GATEWAY_PORT", 30_000),
                listenOnAnyHostAddress: true)
            .UseRedisClustering(redisConnectionString)
            .AddRedisGrainStorageAsDefault(options =>
                options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString!))
            .AddAdoNetGrainStorage("MySqlStore", options =>
            {
                options.Invariant = configuration["Orleans:AdoNetInvariant"];
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
        var redisConnectionString = context.Configuration.GetConnectionString("Redis")!;
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