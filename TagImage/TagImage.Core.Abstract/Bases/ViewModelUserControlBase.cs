namespace TagImage.Core.Abstract.Bases;

public class ViewModelUserControlBase<T> : UserControl where T : class, IDependencyInjection
{
    public T? ViewModel
    {
        get => DataContext as T;
        set => DataContext = value;
    }
}