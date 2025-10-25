namespace NetUtility.Singleton;

public abstract class StaticSingleton<T> where T : new()
{
    private static class Internal
    {
        public static readonly T _instance = new();
    }

    public static T Instance => Internal._instance;
}