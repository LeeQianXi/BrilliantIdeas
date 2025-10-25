using AvaloniaUtility.Services;

namespace TagImage.Core.Abstract.Bases;

public abstract class ViewModelWindowBase<T> : Window, IWindow where T : class, IDependencyInjection
{
    public T? ViewModel
    {
        get => DataContext as T;
        set => DataContext = value;
    }
}