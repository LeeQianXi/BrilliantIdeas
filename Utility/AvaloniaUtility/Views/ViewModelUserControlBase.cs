namespace AvaloniaUtility.Views;

public class ViewModelUserControlBase<T> : UserControl, IDiLogger<T> where T : class, IDependencyInjection
{
    public T? ViewModel
    {
        get => DataContext as T;
        set => DataContext = value;
    }

    public ILogger Logger => ViewModel?.Logger!;
}