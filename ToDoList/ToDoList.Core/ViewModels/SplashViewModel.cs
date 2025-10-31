using AvaloniaUtility;
using ToDoList.Core.Abstract.Bases;
using ToDoList.Core.Abstract.ViewModels;
using ToDoList.Core.Abstract.Views;
using ToDoList.DataBase.Services;

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

    public IEnumerator<YieldInstruction?> LoadGroupInfo()
    {
        var tBgs = _backGroupStorage.GetAllGroupsAsync();
        yield return new WaitForTask(tBgs);
        var bgs = tBgs.Result;
        foreach (var bg in bgs)
        {
            _mainMenuViewModel.AddInitWithGroup(bg);
            yield return null;
        }
    }

    public IEnumerator<YieldInstruction?> LoadTaskInfo()
    {
        var tBls = _backLogStorage.GetAllBackLogsAsync();
        yield return new WaitForTask(tBls);
        var bls = tBls.Result;
        foreach (var bl in bls)
        {
            _mainMenuViewModel.AddInitWithTask(bl);
            yield return null;
        }
    }

    public Task ApplyInitWith()
    {
        return _mainMenuViewModel.ApplyInitWith();
    }
}