namespace NetUtility;

public abstract class LazySingleton<T> where T : new()
{
    private static readonly Lazy<T> _lazy = new(() => new T(),LazyThreadSafetyMode.ExecutionAndPublication);
    public static T Instance => _lazy.Value;
}