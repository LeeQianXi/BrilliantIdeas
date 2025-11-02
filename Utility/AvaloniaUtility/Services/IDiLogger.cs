namespace AvaloniaUtility.Services;

internal interface IDiLogger<out T>
    where T : class, IDependencyInjection
{
    T? ViewModel { get; }
    ILogger Logger { get; }
}