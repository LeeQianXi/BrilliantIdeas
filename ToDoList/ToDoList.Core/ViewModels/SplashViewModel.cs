namespace ToDoList.Core.ViewModels;

internal class SplashViewModel(IServiceProvider serviceProvider) : ViewModelBase, ISplashViewModel
{
    private readonly IBackGroupStorage _backGroupStorage = serviceProvider.GetRequiredService<IBackGroupStorage>();
    private readonly IBackLogStorage _backLogStorage = serviceProvider.GetRequiredService<IBackLogStorage>();
    private readonly IMainMenuViewModel _mainMenuViewModel = serviceProvider.GetRequiredService<IMainMenuViewModel>();
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<SplashViewModel>>();

    public void SplashCompleted()
    {
        Logger.LogInformation("Splash completed");
        var mmv = ServiceProvider.GetRequiredService<IMainMenuView>();
        mmv.IsVisible = true;
    }

    public async IAsyncEnumerator<YieldInstruction?> LoadGroupInfo()
    {
        await foreach (var items in _backGroupStorage.SelectDatasAsync(50))
        foreach (var item in items)
        {
            _mainMenuViewModel.AddInitWithGroup(item);
            yield return null;
        }
    }

    public async IAsyncEnumerator<YieldInstruction?> LoadTaskInfo()
    {
        await foreach (var items in _backLogStorage.SelectDatasAsync(50))
        foreach (var item in items)
        {
            _mainMenuViewModel.AddInitWithTask(item);
            yield return null;
        }
    }

    public Task ApplyInitWith()
    {
        return _mainMenuViewModel.ApplyInitWith();
    }
}