using AvaloniaUtility.Services;
using DIAbstract.Services;

namespace AvaloniaUtility.Views;

public abstract class ViewModelWindowBase<T> : Window, IWindow where T : class, IDependencyInjection
{
    public T? ViewModel
    {
        get => DataContext as T;
        set => DataContext = value;
    }
}