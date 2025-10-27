namespace TagImage;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        ServiceLocator.Instance.ServiceProvider = StartUp.ServiceProvider;
        return AppBuilder.Configure<TagImageApp>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}