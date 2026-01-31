namespace DeadLine.Core;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    public static IServiceProvider ServiceProvider { get; internal set; } = null!;

    public IDeadLineWindow DeadLineWindow => ServiceProvider.GetRequiredService<IDeadLineWindow>();
    public IDeadLineViewModel DeadLineViewModel => ServiceProvider.GetRequiredService<IDeadLineViewModel>();

    public INewDeadLineItemViewModel NewDeadLineItemViewModel =>
        ServiceProvider.GetRequiredService<INewDeadLineItemViewModel>();
}