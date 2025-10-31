namespace NetUtility.Graph;

public interface IGadMap<TKey, out TData>
    where TKey : IEquatable<TKey>
{
    public IGadNode<TKey, TData> RootNode { get; }
}