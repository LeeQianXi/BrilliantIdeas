using System.Drawing;
using Microsoft.Extensions.Logging;
using NetUtility;
using ToDoListDb.Abstract;
using ToDoListDb.Abstract.Model;

namespace ToDoListDb.Core.Services;

public class BackGroupStorage : BaseStorage<BackGroup>, IBackGroupStorage
{
    public IServiceProvider ServiceProvider { get; }
    public ILogger Logger { get; }
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly IReferenceCache<int, BackGroup> _referenceCache = new LruCache<int, BackGroup>();

    public BackGroupStorage(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetRequiredService<ILogger<BackGroupStorage>>();
        Initializing();
    }

    private void Initializing()
    {
        _lock.EnterWriteLock();
        try
        {
            var bgs = Connection.Table<BackGroup>().ToListAsync().Result;
            var dBg = bgs.Find(bg => string.Equals(bg.GroupName, BackGroup.Default.GroupName));
            if (dBg is null)
            {
                var nBi = Connection.InsertAsync(BackGroup.Default).Result;
                BackGroup.Default.GroupId = nBi;
                bgs.Add(BackGroup.Default);
            }
            else
            {
                BackGroup.Default.GroupId = dBg.GroupId;
            }

            _referenceCache.UpdateAll(bgs.Select(bg => (bg.GroupId, bg)));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task<BackGroup> CreateNewGroupAsync(string groupName, string color)
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            var cb = _referenceCache.Values.FirstOrDefault(bg => string.Equals(bg.GroupName, groupName));
            if (cb is not null) return cb;
            _lock.EnterWriteLock();
            try
            {
                cb=BackGroup.CreateNew(groupName, color);
                var id = await Connection.InsertAsync(cb);
                cb.GroupId = id;
                _referenceCache.Update(id, cb);
                return cb;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    public async Task<bool> RemoveGroupAsync(BackGroup group)
    {
        ArgumentNullException.ThrowIfNull(group, nameof(group));
        _lock.EnterUpgradeableReadLock();
        try
        {
            var bg = await Connection.FindAsync<BackGroup>(group.GroupId);
            if (bg is null) return false;

            _lock.EnterWriteLock();
            try
            {
                await Connection.DeleteAsync<BackGroup>(bg.GroupId);
                return true;
            }
            finally
            {
                _referenceCache.Remove(group.GroupId);
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    public async Task<bool> ContainsGroupAsync(string groupName)
    {
        _lock.EnterReadLock();
        try
        {
            if (_referenceCache.Values.FirstOrDefault(bg => string.Equals(bg.GroupName, groupName)) is not null)
                return true;
            var bgs = await Connection.Table<BackGroup>().ToArrayAsync();
            return bgs?.FirstOrDefault(bg => string.Equals(bg.GroupName, groupName)) is not null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<bool> ContainsGroupAsync(int groupId)
    {
        _lock.EnterReadLock();
        try
        {
            if (_referenceCache.Keys.Contains(groupId)) return true;
            var bg = await Connection.FindAsync<BackGroup>(groupId);
            return bg is not null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<BackGroup> GetGroupAsync(string groupName)
    {
        _lock.EnterReadLock();
        try
        {
            var bg = _referenceCache.Values.FirstOrDefault(bg => string.Equals(bg.GroupName, groupName));
            if (bg is not null) return bg;
            var bgs = await Connection.Table<BackGroup>().ToArrayAsync();
            return bgs?.FirstOrDefault(bgi => string.Equals(bgi.GroupName, groupName)) ?? BackGroup.Default;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<string>> GetAllGroupNamesAsync()
    {
        _lock.EnterReadLock();
        try
        {
            var bgs = await Connection.Table<BackGroup>().ToArrayAsync();
            return bgs?.Where(bg => bg is not null).Select(bg => bg.GroupName).ToList() ?? [];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<BackGroup>> GetAllGroupsAsync()
    {
        _lock.EnterReadLock();
        try
        {
            return await Connection.Table<BackGroup>().ToArrayAsync() ?? [BackGroup.Default];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<bool> ChangeGroupNameAsync(int groupId, string newGroupName)
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            var bg = await Connection.FindAsync<BackGroup>(groupId);
            if (bg is null) return false;
            bg.GroupName = newGroupName;
            _lock.EnterWriteLock();
            try
            {
                await Connection.UpdateAsync(bg);
                _referenceCache.Update(bg.GroupId, bg);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    public async Task<bool> ChangeGroupColorAsync(int groupId, int newColor)
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            var bg = await Connection.FindAsync<BackGroup>(groupId);
            if (bg is null) return false;
            bg.ColorArgb = newColor;
            _lock.EnterWriteLock();
            try
            {
                await Connection.UpdateAsync(bg);
                _referenceCache.Update(bg.GroupId, bg);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    public Task<bool> ChangeGroupColorAsync(int groupId, Color newColor) =>
        ChangeGroupColorAsync(groupId, newColor.ToArgb());

    public Task<bool> ChangeGroupColorAsync(int groupId, string newColor) =>
        ChangeGroupColorAsync(groupId, newColor.StringToArgbInt());
}