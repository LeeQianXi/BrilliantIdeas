using System.Collections.Concurrent;
using DataBaseAbstract.Services;

namespace DataBaseAbstract.Storage;

public abstract class BaseStorage<TData>
    where TData : IModelBasic, new()
{
    public readonly string DbName;
    public string UserInfoPath => PathHelper.GetLocalFilePath($"{DbName}.sqlite");

    protected SQLiteAsyncConnection Connection
    {
        get
        {
            if (StorageInternal.StorageConnectionMap.TryGetValue(DbName, out var connection))
                return connection;
            connection = new SQLiteAsyncConnection(UserInfoPath);
            StorageInternal.StorageConnectionMap[DbName] = connection;
            return connection;
        }
    }

    protected BaseStorage(string dbName)
    {
        DbName = dbName;
        Connection.CreateTableAsync<TData>().Wait();
    }
}

internal static class StorageInternal
{
    public static readonly ConcurrentDictionary<string, SQLiteAsyncConnection> StorageConnectionMap = new();
}