namespace TagImage.Core;

public class TagImageApp : Application
{
    private ILogger<TagImageApp> Logger { get; } =
        ServiceLocator.Instance.ServiceProvider.GetRequiredService<ILogger<TagImageApp>>();

    public override void Initialize()
    {
        Logger.LogInformation("Initializing ToDoListApp");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Logger.LogInformation("OnFrameworkInitializationCompleted");

        // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
        // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
        DisableAvaloniaDataAnnotationValidation();

        base.OnFrameworkInitializationCompleted();
        var startup = ServiceLocator.Instance.ServiceProvider.GetRequiredService<IStartupWindow>();
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