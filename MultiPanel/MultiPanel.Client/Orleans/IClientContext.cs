using Microsoft.Extensions.Hosting;

namespace MultiPanel.Client.Orleans;

public interface IClientContext
{
    public IClusterClient Client { get; }
    public IHost ClientHost { get; }
    public IServiceProvider ServiceProvider { get; }
    public bool IsConnected { get; }
    Task<bool> TryConnect();
}