namespace NetUtility.Graph;

public class GadNode<TKey, TData> : IGadNode<TKey, TData>
    where TKey : IEquatable<TKey>
{
    private readonly HashSet<TKey> _children;

    public GadNode(TKey key, TData data, params IEnumerable<TKey> children)
    {
        Key = key;
        Data = data;
        _children = new HashSet<TKey>(children);
    }

    public TKey Key { get; }
    public TData Data { get; }
    public ISet<TKey> Children => _children;

    public void AddChild(TKey childKey)
    {
        _children.Add(childKey);
    }

    public void RemoveChild(TKey childKey)
    {
        _children.Remove(childKey);
    }

    public bool HasChild(TKey childKey)
    {
        return _children.Contains(childKey);
    }

    public void AddChildren(params IEnumerable<TKey> childKeys)
    {
        _children.UnionWith(childKeys);
    }

    public void RemoveChildren(params IEnumerable<TKey> childKeys)
    {
        _children.ExceptWith(childKeys);
    }
}