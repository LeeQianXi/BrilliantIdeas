namespace TagImage.Core.Abstract.Bases;

public abstract class ViewModelBase : ObservableObject, IDependencyInjection
{
    public abstract IServiceProvider ServiceProvider { get; }
    public abstract ILogger Logger { get; }
}