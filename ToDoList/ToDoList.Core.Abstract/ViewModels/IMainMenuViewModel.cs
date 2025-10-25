namespace ToDoList.Core.Abstract.ViewModels;

public interface IMainMenuViewModel : IDependencyInjection, INotifyPropertyChanged
{
    IEnumerable<BackGroup> InitWithGroups { get; }
    IEnumerable<BackLog> InitWithTasks { get; }
    void AddInitWithGroup(BackGroup bg);
    void AddInitWithTask(BackLog bl);
    Task ApplyInitWith();
}