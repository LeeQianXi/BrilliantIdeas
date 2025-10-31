namespace AvaloniaUtility.Models;

public class ViewModelProxy : INotifyPropertyChanged, IDisposable
{
    public object Source { get; }
    private readonly Dictionary<string, PropertyInfo> _props;

    public ViewModelProxy(object source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        _props = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p is { CanRead: true, CanWrite: true }).ToDictionary(p => p.Name, p => p);
    }

    #region 万能属性

    public object? this[string propName]
    {
        get
        {
            if (_props.TryGetValue(propName, out var pi))
            {
                return pi.GetValue(Source);
            }

            return Source.GetType().GetProperty(propName) is { CanRead: true } rp
                ? rp.GetValue(Source)
                : throw new ArgumentException($"Property {propName} not found");
        }
        set
        {
            if (_props.TryGetValue(propName, out var pi))
            {
                var old = pi.GetValue(Source);
                if (Equals(old, value)) return;
                pi.SetValue(Source, value);
                OnPropertyChanged(propName);
                return;
            }

            if (Source.GetType().GetProperty(propName) is not { CanWrite: true } wp)
                throw new ArgumentException($"Property {propName} not found");
            wp.SetValue(Source, value);
            OnPropertyChanged(propName);
            throw new ArgumentException($"Property {propName} not found");
        }
    }

    #endregion

    #region 强类型快捷方式

    public TValue? GetValue<TValue>([CallerMemberName] string? propName = null)
    {
        ArgumentNullException.ThrowIfNull(propName);
        return (TValue?)this[propName];
    }

    public void SetValue<TValue>(TValue? value, [CallerMemberName] string? propName = null)
    {
        ArgumentNullException.ThrowIfNull(propName);
        this[propName] = value;
    }

    #endregion

    public void Dispose()
    {
        // TODO 在此释放托管资源
        PropertyChanged = null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}