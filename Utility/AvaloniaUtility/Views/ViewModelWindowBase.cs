namespace AvaloniaUtility.Views;

public abstract class ViewModelWindowBase<T> : Window, IWindow, IDiLogger<T> where T : class, IDependencyInjection
{
    protected ViewModelWindowBase()
    {
        GetType().GetRuntimeMethod("InitializeComponent", [typeof(bool), typeof(bool)])!.Invoke(this, [true, true]);
    }

    public T? ViewModel
    {
        get => DataContext as T;
        set => DataContext = value;
    }

    public ILogger Logger => ViewModel?.Logger!;
}