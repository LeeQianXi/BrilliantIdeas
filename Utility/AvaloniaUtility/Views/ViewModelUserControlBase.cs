using DIAbstract.Services;

namespace AvaloniaUtility.Views;

public class ViewModelUserControlBase<T> : UserControl where T : class, IDependencyInjection
{
    public T? ViewModel
    {
        get => DataContext as T;
        set => DataContext = value;
    }
}