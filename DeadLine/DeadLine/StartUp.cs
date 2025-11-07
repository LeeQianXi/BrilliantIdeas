namespace DeadLine;

public static class StartUp
{
    [field: MaybeNull]
    [field: AllowNull]
    public static IServiceProvider ServiceProvider => field ??= CreateServiceProvider();

    private static ServiceProvider CreateServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .UseDlManagerOptions()
            .Build();

        var services = new ServiceCollection();

        return services
            .StartUpWith(configuration)
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(configuration)
            .AddLogging(logger => logger.AddConsole())
            .AddOptions()
            .BuildServiceProvider();
    }

    private static IServiceCollection StartUpWith(this IServiceCollection collection, IConfiguration configuration)
    {
        return collection
            .UseAvaloniaCore<DeadLineWindow>()
            .UseDeadLineCore();
    }

    private static IConfigurationBuilder UseDlManagerOptions(this IConfigurationBuilder builder)
    {
        return builder
            .AddJsonFile("appsettings.json", false, true);
    }
}