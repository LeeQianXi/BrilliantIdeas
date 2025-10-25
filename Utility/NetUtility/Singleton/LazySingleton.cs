namespace NetUtility.Singleton;

public abstract class LazySingleton<T> where T : new()
{
    private static readonly Lazy<T> Lazy = new(() => new T(),LazyThreadSafetyMode.ExecutionAndPublication);
    public static T Instance => Lazy.Value;
}