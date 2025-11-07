namespace NetUtility.HFsm;

internal sealed class HFsmManager : NetUtilityModule, IHFsmManager
{
    private readonly Dictionary<TypeNamePair, HFsmBase> _hfsms = new();
    private readonly List<HFsmBase> _tempFsms = [];

    /// <summary>
    ///     获取游戏框架模块优先级。
    /// </summary>
    /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
    internal override int Priority => 1;

    /// <summary>
    ///     获取有限状态机数量。
    /// </summary>
    public int Count => _hfsms.Count;

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <returns>是否存在有限状态机。</returns>
    public bool HasHFsm<T>() where T : class
    {
        return InternalHasHFsm(new TypeNamePair(typeof(T)));
    }

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <returns>是否存在有限状态机。</returns>
    public bool HasHFsm(Type ownerType)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalHasHFsm(new TypeNamePair(ownerType));
    }

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>是否存在有限状态机。</returns>
    public bool HasHFsm<T>(string name) where T : class
    {
        return InternalHasHFsm(new TypeNamePair(typeof(T), name));
    }

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>是否存在有限状态机。</returns>
    public bool HasHFsm(Type ownerType, string name)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalHasHFsm(new TypeNamePair(ownerType, name));
    }

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <returns>要获取的有限状态机。</returns>
    public IHFsm<T>? GetHFsm<T>() where T : class
    {
        return InternalGetHFsm(new TypeNamePair(typeof(T))) as IHFsm<T>;
    }

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <returns>要获取的有限状态机。</returns>
    public HFsmBase? GetHFsm(Type ownerType)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalGetHFsm(new TypeNamePair(ownerType));
    }

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>要获取的有限状态机。</returns>
    public IHFsm<T>? GetHFsm<T>(string name) where T : class
    {
        return InternalGetHFsm(new TypeNamePair(typeof(T), name)) as IHFsm<T>;
    }

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>要获取的有限状态机。</returns>
    public HFsmBase? GetHFsm(Type ownerType, string name)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalGetHFsm(new TypeNamePair(ownerType, name));
    }

    /// <summary>
    ///     获取所有有限状态机。
    /// </summary>
    /// <returns>所有有限状态机。</returns>
    public HFsmBase[] GetAllHFsms()
    {
        return _hfsms.Select(p => p.Value).ToArray();
    }

    /// <summary>
    ///     获取所有有限状态机。
    /// </summary>
    /// <param name="results">所有有限状态机。</param>
    public void GetAllHFsms(out List<HFsmBase> results)
    {
        results = _hfsms.Select(p => p.Value).ToList();
    }

    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="rootState">有限状态机状态集合。</param>
    /// <returns>要创建的有限状态机。</returns>
    public IHFsm<T> CreateHFsm<T>(T owner, HFsmState<T> rootState) where T : class
    {
        return CreateHFsm(string.Empty, owner, rootState);
    }

    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">有限状态机名称。</param>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="rootState">有限状态机状态集合。</param>
    /// <returns>要创建的有限状态机。</returns>
    public IHFsm<T> CreateHFsm<T>(string name, T owner, HFsmState<T> rootState) where T : class
    {
        var typeNamePair = new TypeNamePair(typeof(T), name);
        if (HasHFsm<T>(name))
            throw new NetUtilityException(Utility.Text.Format("Already exist HFsm '{0}'.", typeNamePair));

        var hFsm = HFsm<T>.Create(name, owner, rootState);
        _hfsms.Add(typeNamePair, hFsm);
        return hFsm;
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyHFsm<T>() where T : class
    {
        return InternalDestroyHFsm(new TypeNamePair(typeof(T)));
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyHFsm(Type ownerType)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalDestroyHFsm(new TypeNamePair(ownerType));
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">要销毁的有限状态机名称。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyHFsm<T>(string name) where T : class
    {
        return InternalDestroyHFsm(new TypeNamePair(typeof(T), name));
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <param name="name">要销毁的有限状态机名称。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyHFsm(Type ownerType, string name)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalDestroyHFsm(new TypeNamePair(ownerType, name));
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="hFsm">要销毁的有限状态机。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyHFsm<T>(IHFsm<T> hFsm) where T : class
    {
        return hFsm == null
            ? throw new NetUtilityException("HFsm is invalid.")
            : InternalDestroyHFsm(new TypeNamePair(typeof(T), hFsm.Name));
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <param name="hFsm">要销毁的有限状态机。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyHFsm(HFsmBase hFsm)
    {
        return hFsm == null
            ? throw new NetUtilityException("HFsm is invalid.")
            : InternalDestroyHFsm(new TypeNamePair(hFsm.OwnerType, hFsm.Name));
    }


    /// <summary>
    ///     有限状态机管理器轮询。
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    internal override void Update(float elapseSeconds, float realElapseSeconds)
    {
        _tempFsms.Clear();
        if (_hfsms.Count <= 0) return;
        foreach (var fsm in _hfsms) _tempFsms.Add(fsm.Value);
        foreach (var fsm in _tempFsms.Where(fsm => !fsm.IsDestroyed)) fsm.Update(elapseSeconds, realElapseSeconds);
    }

    /// <summary>
    ///     关闭并清理有限状态机管理器。
    /// </summary>
    internal override void Shutdown()
    {
        foreach (var fsm in _hfsms) fsm.Value.Shutdown();

        _hfsms.Clear();
        _tempFsms.Clear();
    }

    private bool InternalHasHFsm(TypeNamePair typeNamePair)
    {
        return _hfsms.ContainsKey(typeNamePair);
    }

    private HFsmBase? InternalGetHFsm(TypeNamePair typeNamePair)
    {
        return _hfsms.GetValueOrDefault(typeNamePair);
    }

    private bool InternalDestroyHFsm(TypeNamePair typeNamePair)
    {
        if (!_hfsms.TryGetValue(typeNamePair, out var hFsm)) return false;
        hFsm.Shutdown();
        return _hfsms.Remove(typeNamePair);
    }
}