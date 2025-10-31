namespace DLManager.Core.Abstract.Bases;

public abstract partial class ViewModelBase : ReactiveObject, IDependencyInjection
{
    public abstract IServiceProvider ServiceProvider { get; }
    public abstract ILogger Logger { get; }
}