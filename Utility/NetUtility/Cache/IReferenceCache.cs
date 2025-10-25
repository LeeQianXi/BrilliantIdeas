namespace NetUtility.Cache;

public interface IReferenceCache<TKey, TValue> where TKey : notnull where TValue : class
{
    /// <summary>
    /// 当前缓存项数量。
    /// </summary>
    int Count { get; }

    /// <summary>
    /// 所有缓存键
    /// </summary>
    IReadOnlyCollection<TKey> Keys { get; }
    
    /// <summary>
    /// 所有缓存值
    /// </summary>
    IReadOnlyCollection<TValue> Values { get; }

    /// <summary>
    /// 获取或添加。若键存在则返回缓存值并更新顺序；若不存在则调用 valueFactory 加入缓存。
    /// </summary>
    TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);

    /// <summary>
    /// 尝试获取，不更新顺序。
    /// </summary>
    bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value);

    /// <summary>
    /// 显式移除。
    /// </summary>
    bool Remove(TKey key);

    /// <summary>
    /// 若键存在，则更新其值并将它标记为最新使用；否则返回 false。
    /// </summary>
    void Update(TKey key, TValue newValue);

    /// <summary>
    /// 若键存在，则更新其值并将它标记为最新使用；否则返回 false。
    /// </summary>
    void UpdateAll(IEnumerable<(TKey, TValue)> pairs);
    /// <summary>
    /// 清空缓存。
    /// </summary>
    void Clear();
}