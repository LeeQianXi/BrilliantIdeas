namespace DeadLine.Core.ViewModel;

public class DeadLineViewModel(IServiceProvider serviceProvider) : ViewModelBase, IDeadLineViewModel
{
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<DeadLineViewModel>>();
}