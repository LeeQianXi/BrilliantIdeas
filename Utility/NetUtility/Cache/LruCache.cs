namespace NetUtility.Cache;

public class LruCache<TKey, TValue> : IReferenceCache<TKey, TValue> where TKey : notnull where TValue : class
{
    private readonly int _capacity;
    private readonly LinkedList<LruItem> _list;
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<TKey, LinkedListNode<LruItem>> _map;

    public LruCache(int maxCapacity = 50)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCapacity);
        _capacity = maxCapacity;
        _map = new Dictionary<TKey, LinkedListNode<LruItem>>(maxCapacity);
        _list = [];
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _map.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public IReadOnlyCollection<TKey> Keys
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _map.Keys;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public IReadOnlyCollection<TValue> Values
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _map.Values.Select(nv => nv.Value.Value).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        // 先尝试读
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_map.TryGetValue(key, out var node))
            {
                // 命中，移到队尾
                _list.Remove(node);
                _list.AddLast(node);
                return node.Value.Value;
            }

            // 未命中，需要写入
            var value = valueFactory(key);
            _lock.EnterWriteLock();
            try
            {
                // 双检
                if (_map.TryGetValue(key, out node))
                {
                    _list.Remove(node);
                    _list.AddLast(node);
                    return node.Value.Value;
                }

                // 淘汰最久未使用
                if (_map.Count == _capacity)
                {
                    var first = _list.First!;
                    _map.Remove(first.Value.Key);
                    _list.RemoveFirst();
                }

                var item = new LruItem(key, value);
                node = _list.AddLast(item);
                _map[key] = node;
                return value;
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

    public void Update(TKey key, TValue value)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_map.TryGetValue(key, out var node))
            {
                node.Value = new LruItem(key, value);
                _list.Remove(node);
                _list.AddLast(node);
            }
            else
            {
                node = _list.AddLast(new LruItem(key, value));
                _map[key] = node;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UpdateAll(IEnumerable<(TKey, TValue)> pairs)
    {
        _lock.EnterWriteLock();
        try
        {
            foreach (var (k, v) in pairs)
                if (_map.TryGetValue(k, out var node))
                {
                    node.Value = new LruItem(k, v);
                    _list.Remove(node);
                    _list.AddLast(node);
                }
                else
                {
                    node = _list.AddLast(new LruItem(k, v));
                    _map[k] = node;
                }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        _lock.EnterReadLock();
        try
        {
            if (_map.TryGetValue(key, out var node))
            {
                value = node.Value.Value;
                return true;
            }

            value = null;
            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool Remove(TKey key)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_map.Remove(key, out var node)) return false;
            _list.Remove(node);
            return true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _map.Clear();
            _list.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _lock?.Dispose();
    }

    #region 私有

    private sealed record LruItem(TKey Key, TValue Value);

    #endregion
}