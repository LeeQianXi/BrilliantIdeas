using SQLite;
using ToDoListDb.Abstract.Helper;

namespace ToDoListDb.Abstract;

public abstract class BaseStorage<TData>
    where TData : new()
{
    public static string DbName => StorageData.DbName;
    public static string UserInfoPath => StorageData.UserInfoPath;
    protected static SQLiteAsyncConnection Connection => StorageData.Connection;

    protected BaseStorage()
    {
        Connection.CreateTableAsync<TData>().Wait();
    }
}

internal static class StorageData
{
    public const string DbName = "ToDoList.db";
    public static string UserInfoPath => PathHelper.GetLocalFilePath(DbName);

    public static readonly SQLiteAsyncConnection Connection = new(UserInfoPath);
}