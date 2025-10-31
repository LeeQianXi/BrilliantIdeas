namespace AvaloniaUtility.Views;

public class ViewModelUserControlBase<T> : UserControl, IDiLogger<T> where T : class, IDependencyInjection
{
    protected ViewModelUserControlBase()
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