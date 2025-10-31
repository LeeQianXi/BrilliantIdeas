namespace AvaloniaUtility.Views;

public abstract class ViewModelWindowBase<T> : Window, IWindow, IDiLogger<T> where T : class, IDependencyInjection
{
    public T? ViewModel
    {
        get => DataContext as T;
        set => DataContext = value;
    }

    public ILogger Logger => ViewModel?.Logger!;
}