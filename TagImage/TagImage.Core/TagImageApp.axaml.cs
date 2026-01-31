namespace TagImage.Core;

public class TagImageApp(ILogger<TagImageApp> logger, IServiceProvider serviceProvider) : Application
{
    public override void Initialize()
    {
        ServiceLocator.ServiceProvider = serviceProvider;
        logger.LogInformation("Initializing ToDoListApp");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        logger.LogInformation("OnFrameworkInitializationCompleted");

        // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
        // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
        DisableAvaloniaDataAnnotationValidation();

        base.OnFrameworkInitializationCompleted();
        var startup = serviceProvider.GetRequiredService<IStartupWindow>();
        startup.Show();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove) BindingPlugins.DataValidators.Remove(plugin);
    }
}