using ToDoList.Core.Views;
using ToDoListDb.Core;

namespace ToDoList;

public static class StartUp
{
    [field: MaybeNull, AllowNull] public static IServiceProvider ServiceProvider => field ??= CreateServiceProvider();

    private static ServiceProvider CreateServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .UseTechTreeOptions()
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

    private static IServiceCollection StartUpWith(this IServiceCollection services, IConfiguration configuration)
    {
        return services.UseAvaloniaCore<ToDoListWindow>().UseToDoListCore().UseToDoListDbCore();
    }

    private static IConfigurationBuilder UseTechTreeOptions(this IConfigurationBuilder builder)
    {
        return builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    }
}