using Microsoft.Extensions.Logging;

namespace DIAbstract;

public interface IDependencyInjection
{
    IServiceProvider ServiceProvider { get; }
    ILogger Logger { get; }
}