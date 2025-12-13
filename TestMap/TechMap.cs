using TechNode = NetUtility.Graph.IGadNode<System.Guid, TestMap.TechMap.TechNodeData>;

namespace TestMap;

public partial class TechMap : IGadMap<Guid, TechMap.TechNodeData>
{
    private readonly ConcurrentDictionary<Guid, TechNode> _map;

    public TechMap()
    {
        RootNode = new GadNode<Guid, TechNodeData>(Guid.Empty, TechNodeData.Empty);
        _map = new ConcurrentDictionary<Guid, TechNode>
        {
            [RootNode.Key] = RootNode
        };
    }

    public TechMap(params IEnumerable<TechNode> children)
    {
        _map = new ConcurrentDictionary<Guid, TechNode>(children.ToDictionary(x => x.Key, y => y));
        if (_map.IsEmpty)
        {
            RootNode = new GadNode<Guid, TechNodeData>(Guid.Empty, TechNodeData.Empty);
            _map[RootNode.Key] = RootNode;
            return;
        }

        var roots = FindRootNode(_map);
        switch (roots.Count)
        {
            case 0:
                throw new ArgumentException("This is a cyclic graph");
            case 1:
                RootNode = _map[roots.First()];
                break;
            default:
                RootNode = new GadNode<Guid, TechNodeData>(Guid.Empty, TechNodeData.Empty);
                RootNode.Children.UnionWith(roots);
                break;
        }
    }

    public IEnumerable<TechNode> Nodes => _map.Values;

    public TechNode this[Guid key] => _map.GetValueOrDefault(key, RootNode);
    public TechNode RootNode { get; }

    [Pure]
    private static ISet<Guid> FindRootNode(IDictionary<Guid, TechNode> map)
    {
        return map.Keys.Except(map.SelectMany(kv => kv.Value.Children)).ToHashSet();
    }

    public Guid AddNode(TechNodeData data)
    {
        var child = new GadNode<Guid, TechNodeData>(Guid.NewGuid(), data);
        _map[child.Key] = child;
        RootNode.Children.Add(child.Key);
        return child.Key;
    }

    public IEnumerable<Guid> AddNodes(params IEnumerable<TechNodeData> datas)
    {
        var children = datas.Select(td => new GadNode<Guid, TechNodeData>(Guid.NewGuid(), td)).ToArray();
        foreach (var child in children)
        {
            _map[child.Key] = child;
            RootNode.Children.Add(child.Key);
        }

        return children.Select(td => td.Key);
    }

    public void RemoveNode(Guid key)
    {
        if (!_map.TryGetValue(key, out var parent))
            throw new KeyNotFoundException($"Parent key {key} does not exist");
        var parents = _map.Values.AsParallel().Where(v => v.Children.Contains(key)).ToArray();
        foreach (var gadNode in parents) gadNode.Children.Remove(key);
    }

    public void RemoveNode(params IEnumerable<Guid> keys)
    {
        var ksa = keys.ToHashSet();
        var parents = ksa.ToArray()
            .Where(k => _map.ContainsKey(k))
            .AsParallel()
            .SelectMany(k => _map.Values.Where(v => v.Children.Contains(k)))
            .DistinctBy(k => k.Key)
            .ToArray();
        foreach (var key in ksa.Where(k => _map.ContainsKey(k))) _map.Remove(key, out _);

        foreach (var gadNode in parents) gadNode.Children.ExceptWith(ksa);
    }

    public void AddChild(Guid parentKey, Guid childKey)
    {
        if (!_map.TryGetValue(parentKey, out var parent))
            throw new KeyNotFoundException($"Parent key {parentKey} does not exist");
        if (!_map.ContainsKey(childKey))
            throw new KeyNotFoundException($"Child key {childKey} does not exist");
        RootNode.Children.Remove(childKey);
        parent.Children.Add(childKey);
    }

    public void AddChildren(Guid parentKey, params IEnumerable<Guid> childKey)
    {
        if (!_map.TryGetValue(parentKey, out var parent))
            throw new KeyNotFoundException($"Parent key {parentKey} does not exist");
        var guids = childKey as Guid[] ?? childKey.ToArray();
        RootNode.Children.ExceptWith(guids);
        parent.Children.UnionWith(guids.Where(k => _map.ContainsKey(k)));
    }
}