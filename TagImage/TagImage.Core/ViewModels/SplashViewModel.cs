namespace TagImage.Core.ViewModels;

public class SplashViewModel(IServiceProvider serviceProvider) : ViewModelBase, ISplashViewModel
{
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<SplashViewModel>>();

    public void SplashCompleted()
    {
    }
}