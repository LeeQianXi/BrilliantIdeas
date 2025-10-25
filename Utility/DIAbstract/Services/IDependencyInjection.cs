using Microsoft.Extensions.Logging;

namespace DIAbstract.Services;

public interface IDependencyInjection
{
    IServiceProvider ServiceProvider { get; }
    ILogger Logger { get; }
}