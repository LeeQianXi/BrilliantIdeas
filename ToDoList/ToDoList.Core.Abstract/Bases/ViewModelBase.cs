namespace ToDoList.Core.Abstract.Bases;

public abstract partial class ViewModelBase : ObservableObject, IDependencyInjection
{
    public abstract IServiceProvider ServiceProvider { get; }
    public abstract ILogger Logger { get; }
}