using DIAbstract.Services;
using Microsoft.Extensions.Logging;
using NetUtility;
using ToDoListDb.Abstract;
using ToDoListDb.Abstract.Model;

namespace ToDoListDb.Core.Services;

public class BackLogStorage(IServiceProvider serviceProvider)
    : BaseStorage<BackLog>, IBackLogStorage, IDependencyInjection
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<BackLogStorage>>();

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly IReferenceCache<int, BackLog> _referenceCache = new LruCache<int, BackLog>();

    public async Task<BackLog> CreateNewBackLogAsync(string title, string? description, BackGroup? group)
    {
        _lock.EnterWriteLock();
        group ??= BackGroup.Default;
        try
        {
            var bl = BackLog.CreateNew(title, description, group);
            var id = await Connection.InsertAsync(bl);
            bl.TaskId = id;
            _referenceCache.Update(id, bl);
            return bl;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task<bool> DeleteBackLogAsync(int taskId)
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            var bl = await Connection.FindAsync<BackLog>(taskId);
            if (bl is null) return false;
            _lock.EnterWriteLock();
            try
            {
                await Connection.DeleteAsync<BackLog>(taskId);
                return true;
            }
            finally
            {
                _referenceCache.Remove(taskId);
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    public async Task DeleteAllBackLogsAsync(BackGroup? group = null)
    {
        _lock.EnterUpgradeableReadLock();
        group ??= BackGroup.Default;
        try
        {
            var bls = await Connection.Table<BackLog>().ToArrayAsync();
            var rBls = (bls?.Where(bl => bl is not null && bl.GroupId == group.GroupId) ?? []).ToArray();
            _lock.EnterWriteLock();
            try
            {
                foreach (var rBl in rBls)
                {
                    await Connection.DeleteAsync<BackLog>(rBl.TaskId);
                }
            }
            finally
            {
                foreach (var rBl in rBls)
                {
                    _referenceCache.Remove(rBl.TaskId);
                }

                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    public async Task UpdateBackLogAsync(BackLog newBackLog)
    {
        _lock.EnterWriteLock();
        try
        {
            await Connection.UpdateAsync(newBackLog);
            var ni = await Connection.UpdateAsync(newBackLog);
            newBackLog.GroupId = ni;
            _referenceCache.Update(ni, newBackLog);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task<BackLog> GetBackLogAsync(int taskId)
    {
        _lock.EnterReadLock();
        try
        {
            if (_referenceCache.TryGetValue(taskId, out var backLog))
            {
                return backLog;
            }

            var bl = await Connection.FindAsync<BackLog>(taskId);
            if (bl is null) return BackLog.Empty;
            _referenceCache.Update(bl.GroupId, bl);
            return bl;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<BackLog>> GetAllBackLogsAsync(BackGroup? group)
    {
        group ??= BackGroup.Default;
        _lock.EnterReadLock();
        try
        {
            var bls = await Connection.Table<BackLog>().ToArrayAsync();
            return bls?.Where(bl => bl.GroupId == group.GroupId) ?? [];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}