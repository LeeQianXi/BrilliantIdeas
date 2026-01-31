using System.Runtime.CompilerServices;

namespace DataBaseAbstract.Storage;

[Obsolete("Will be change to EFCore")]
public abstract class StorageBasic<TData>(string dbName) : BaseStorage<TData>(dbName), IStorageBasic<TData>
    where TData : IModelBasic, new()
{
    protected readonly ReaderWriterLockSlim Lock = new();

    public virtual async Task<int> InsertDataAsync(TData value, CancellationToken token = default)
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

    public virtual async Task InsertDataAsync(CancellationToken token = default, params IEnumerable<TData> values)
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

    public virtual async Task<TData> GetDataAsync(int key, CancellationToken token = default)
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

    public virtual async Task<TV> GetDataAsync<TV>(int key, Expression<Func<TData, TV>> select,
        CancellationToken token = default)
    {
        Lock.EnterReadLock();
        try
        {
            var ret = await Connection.Table<TData>().FirstOrDefaultAsync(d => d.PrimaryKey == key);
            return ret is null ? throw new KeyNotFoundException() : select.Compile().Invoke(ret);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public virtual async Task<TData?> FindDataAsync(int key, CancellationToken token = default)
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

    public virtual async Task<TV?> FindDataAsync<TV>(int key, Expression<Func<TData, TV>> select,
        CancellationToken token = default)
    {
        Lock.EnterReadLock();
        try
        {
            var ret = await Connection.Table<TData>().FirstOrDefaultAsync(d => d.PrimaryKey == key);
            return ret is null ? default : select.Compile().Invoke(ret);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public async IAsyncEnumerable<IEnumerable<TData>> SelectDatasAsync(int limit = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        if (limit < 0) throw new ArgumentException("limit cannot be less than zero", nameof(limit));
        var table = Connection.Table<TData>();
        if (table is null) yield break;
        if (limit is 0)
        {
            Lock.EnterReadLock();
            try
            {
                yield return await table.ToListAsync();
                yield break;
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        Lock.EnterReadLock();
        try
        {
            do
            {
                yield return await table.Take(limit).ToListAsync();
                table = table.Skip(limit);
            } while (await table.CountAsync() > 0);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public async IAsyncEnumerable<IEnumerable<TV>> SelectDatasAsync<TV>(
        Expression<Func<TData, TV>> select, int limit = 0, [EnumeratorCancellation] CancellationToken token = default)
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
                yield return (await rets.ToArrayAsync()).AsQueryable().Select(select);
                yield break;
            }

            do
            {
                yield return (await rets.Take(limit).ToArrayAsync()).AsQueryable().Select(select);
                rets = rets.Skip(limit);
            } while (await rets.CountAsync() > 0);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public virtual async IAsyncEnumerable<IEnumerable<TData>> SelectDatasAsync(Expression<Func<TData, bool>> predicate,
        int limit = 0, [EnumeratorCancellation] CancellationToken token = default)
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
        Expression<Func<TData, TV>> select, int limit = 0, [EnumeratorCancellation] CancellationToken token = default)
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
                yield return (await rets.ToListAsync()).AsQueryable().Select(select);
                yield break;
            }

            do
            {
                yield return (await rets.Take(limit).ToListAsync()).AsQueryable().Select(select);
                rets = rets.Skip(limit);
            } while (await rets.CountAsync() > 0);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public virtual async Task UpdateDataAsync(TData value, CancellationToken token = default)
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

    public virtual async Task UpdateDataAsync(CancellationToken token = default, params IEnumerable<TData> values)
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

    public virtual async Task<TData> DeleteDataAsync(int key, CancellationToken token = default)
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

    public virtual async Task DeleteDataAsync(CancellationToken token = default, params IEnumerable<int> keys)
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

    public virtual async Task DeleteDataAsync(Expression<Func<TData, bool>> predicate,
        CancellationToken token = default)
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

    public virtual async Task ClearTableAsync(CancellationToken token = default)
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

    public virtual async Task BeginTransactionAsync(Action<SQLiteConnection> action, CancellationToken token = default)
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