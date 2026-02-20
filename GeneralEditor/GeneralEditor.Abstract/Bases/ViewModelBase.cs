using DIAbstract;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace GeneralEditor.Core.Abstract.Bases;

public abstract class ViewModelBase : ReactiveObject, IDependencyInjection
{
    public abstract IServiceProvider ServiceProvider { get; }
    public abstract ILogger Logger { get; }
}