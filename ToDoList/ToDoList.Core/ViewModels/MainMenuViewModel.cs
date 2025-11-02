namespace ToDoList.Core.ViewModels;

internal class MainMenuViewModel(IServiceProvider serviceProvider, ILogger<MainMenuViewModel> logger)
    : ViewModelBase, IMainMenuViewModel
{
    private readonly List<BackGroup> _backGroups = [];
    private readonly List<BackLog> _backLogs = [];
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = logger;
    public IEnumerable<BackGroup> InitWithGroups => _backGroups;
    public IEnumerable<BackLog> InitWithTasks => _backLogs;

    public void AddInitWithGroup(BackGroup bg)
    {
        _backGroups.Add(bg);
    }

    public void AddInitWithTask(BackLog bl)
    {
        _backLogs.Add(bl);
    }

    public async Task ApplyInitWith()
    {
    }
}