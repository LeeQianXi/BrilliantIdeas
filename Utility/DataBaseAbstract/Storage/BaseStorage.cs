using DIAbstract;

namespace DataBaseAbstract.Storage;

public abstract class BaseStorage<TData>(string dbName) : IAsyncLifecycle
    where TData : IModelBasic, new()
{
    public string DbName { get; } = dbName;

    public string UserInfoPath => StorageInternal.ResolvePath(DbName);

    protected SQLiteAsyncConnection Connection =>
        StorageInternal.StorageConnectionMap.GetOrAdd(DbName,
            k => new SQLiteAsyncConnection(StorageInternal.ResolvePath(k)));

    public async Task InitializeAsync()
    {
        await Connection.CreateTableAsync<TData>();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

internal static class StorageInternal
{
    public static readonly ConcurrentDictionary<string, SQLiteAsyncConnection> StorageConnectionMap = new();

    public static string ResolvePath(string dbName)
    {
        return PathHelper.GetLocalFilePath($"{dbName}.sqlite");
    }
}