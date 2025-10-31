namespace NetUtility.Graph;

public interface IGadNode<TKey, out TData>
    where TKey : IEquatable<TKey>
{
    public TKey Key { get; }
    public TData Data { get; }
    public ISet<TKey> Children { get; }
}