using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AvaloniaUtility.Models;
using CommunityToolkit.Mvvm.Input;
using ToDoList.Core.Abstract.ViewModels;
using ToDoList.Core.Abstract.Views;
using ToDoListCore.Abstract.Services;
using ToDoListDb.Abstract.Model;

namespace ToDoListCore.ViewModels;

internal class MainMenuViewModel(IServiceProvider serviceProvider, ILogger<MainMenuViewModel> logger)
    : ViewModelBase, IMainMenuViewModel
{
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = logger;

    private readonly List<BackGroup> _backGroups = [];
    private readonly List<BackLog> _backLogs = [];
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