namespace DIAbstract;

public interface IAsyncLifecycle
{
    Task InitializeAsync();
    Task DisposeAsync();
}