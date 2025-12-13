using System.Linq.Expressions;

namespace DataBaseAbstract.Storage;

public abstract class StorageBasic<TData>(string dbName) : BaseStorage<TData>(dbName), IStorageBasic<TData>
    where TData : IModelBasic, new()
{
    protected readonly ReaderWriterLockSlim Lock = new();

    public virtual async Task<int> InsertDataAsync(TData value)
    {
        Lock.EnterWriteLock();
        try
        {
            return await Connection.InsertAsync(value);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    public virtual async Task InsertDataAsync(params IEnumerable<TData> values)
    {
        Lock.EnterWriteLock();
        try
        {
            _ = await Connection.InsertAllAsync(values);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    public virtual async Task<TData> GetDataAsync(int key)
    {
        Lock.EnterReadLock();
        try
        {
            var ret = await Connection.FindAsync<TData>(key);
            return ret ?? throw new KeyNotFoundException();
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public virtual async Task<TV> GetDataAsync<TV>(int key, IStorageBasic<TData>.Transform<TData, TV> select)
    {
        return select.Invoke(await GetDataAsync(key));
    }

    public virtual async Task<TData?> FindDataAsync(int key)
    {
        Lock.EnterReadLock();
        try
        {
            return await Connection.FindAsync<TData>(key);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public virtual async Task<TV?> FindDataAsync<TV>(int key, IStorageBasic<TData>.Transform<TData, TV> select)
    {
        var ret = await FindDataAsync(key);
        return ret is null ? default : select.Invoke(ret);
    }

    public async IAsyncEnumerable<IEnumerable<TData>> SelectDatasAsync(int limit = 0)
    {
        if (limit < 0) throw new ArgumentException("limit cannot be less than zero", nameof(limit));
        Lock.EnterReadLock();
        try
        {
            var rets = Connection.Table<TData>();
            if (rets is null) yield break;
            if (limit is 0)
            {
                yield return await rets.ToListAsync();
                yield break;
            }

            do
            {
                yield return await rets.Take(limit).ToListAsync();
                rets = rets.Skip(limit);
            } while (await rets.CountAsync() > 0);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public async IAsyncEnumerable<IEnumerable<TV>> SelectDatasAsync<TV>(
        IStorageBasic<TData>.Transform<TData, TV> select, int limit = 0)
    {
        ArgumentNullException.ThrowIfNull(select);
        if (limit < 0) throw new ArgumentException("limit cannot be less than zero", nameof(limit));
        Lock.EnterReadLock();
        try
        {
            var rets = Connection.Table<TData>();
            if (rets is null) yield break;
            if (limit is 0)
            {
                yield return (await rets.ToListAsync()).Select(select.Invoke);
                yield break;
            }

            do
            {
                yield return (await rets.Take(limit).ToListAsync()).Select(select.Invoke);
                rets = rets.Skip(limit);
            } while (await rets.CountAsync() > 0);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public virtual async IAsyncEnumerable<IEnumerable<TData>> SelectDatasAsync(Expression<Func<TData, bool>> predicate,
        int limit = 0)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (limit < 0) throw new ArgumentException("limit cannot be less than zero", nameof(limit));
        Lock.EnterReadLock();
        try
        {
            var rets = Connection.Table<TData>().Where(predicate);
            if (rets is null) yield break;
            if (limit is 0)
            {
                yield return await rets.ToListAsync();
                yield break;
            }

            do
            {
                yield return await rets.Take(limit).ToListAsync();
                rets = rets.Skip(limit);
            } while (await rets.CountAsync() > 0);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public virtual async IAsyncEnumerable<IEnumerable<TV>> SelectDatasAsync<TV>(Expression<Func<TData, bool>> predicate,
        IStorageBasic<TData>.Transform<TData, TV> select, int limit = 0)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(select);
        if (limit < 0) throw new ArgumentException("limit cannot be less than zero", nameof(limit));
        Lock.EnterReadLock();
        try
        {
            var rets = Connection.Table<TData>().Where(predicate);
            if (rets is null) yield break;
            if (limit is 0)
            {
                yield return (await rets.ToListAsync()).Select(select.Invoke);
                yield break;
            }

            do
            {
                yield return (await rets.Take(limit).ToListAsync()).Select(select.Invoke);
                rets = rets.Skip(limit);
            } while (await rets.CountAsync() > 0);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public virtual async Task UpdateDataAsync(TData value)
    {
        Lock.EnterUpgradeableReadLock();
        try
        {
            if (await Connection.FindAsync<TData>(value.PrimaryKey) is null) throw new KeyNotFoundException();
            Lock.EnterWriteLock();
            try
            {
                await Connection.UpdateAsync(value);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
        finally
        {
            Lock.ExitUpgradeableReadLock();
        }
    }

    public virtual async Task UpdateDataAsync(params IEnumerable<TData> values)
    {
        Lock.EnterWriteLock();
        try
        {
            await Connection.UpdateAllAsync(values);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    public virtual async Task<TData> DeleteDataAsync(int key)
    {
        Lock.EnterUpgradeableReadLock();
        try
        {
            var ret = await Connection.FindAsync<TData>(key);
            if (ret is null) throw new KeyNotFoundException();
            Lock.EnterWriteLock();
            try
            {
                await Connection.DeleteAsync<TData>(key);
                return ret;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
        finally
        {
            Lock.ExitUpgradeableReadLock();
        }
    }

    public virtual async Task DeleteDataAsync(params IEnumerable<int> keys)
    {
        Lock.EnterWriteLock();
        try
        {
            await Connection.DeleteAsync<TData>(keys);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    public virtual async Task DeleteDataAsync(Expression<Func<TData, bool>> predicate)
    {
        Lock.EnterWriteLock();
        try
        {
            await Connection.RunInTransactionAsync(con =>
            {
                con.BeginTransaction();
                con.Table<TData>().Where(predicate).Delete();
                con.Commit();
            });
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    public virtual async Task ClearTableAsync()
    {
        Lock.EnterWriteLock();
        try
        {
            await Connection.DeleteAllAsync<TData>();
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    public virtual async Task BeginTransactionAsync(Action<SQLiteConnection> action)
    {
        Lock.EnterWriteLock();
        try
        {
            await Connection.RunInTransactionAsync(action);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }
}