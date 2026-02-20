namespace GeneralEditor;

public class ServiceLocator
{
    public static IServiceProvider ServiceProvider { get; internal set; } = null!;
}