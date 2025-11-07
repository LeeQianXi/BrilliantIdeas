namespace NetUtility.Fsm;

/// <summary>
///     有限状态机管理器。
/// </summary>
internal sealed class FsmManager : NetUtilityModule, IFsmManager
{
    private readonly Dictionary<TypeNamePair, FsmBase> _fsms = new();
    private readonly List<FsmBase> _tempFsms = [];

    /// <summary>
    ///     获取游戏框架模块优先级。
    /// </summary>
    /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
    internal override int Priority => 1;

    /// <summary>
    ///     获取有限状态机数量。
    /// </summary>
    public int Count => _fsms.Count;

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <returns>是否存在有限状态机。</returns>
    public bool HasFsm<T>() where T : class
    {
        return InternalHasFsm(new TypeNamePair(typeof(T)));
    }

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <returns>是否存在有限状态机。</returns>
    public bool HasFsm(Type ownerType)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalHasFsm(new TypeNamePair(ownerType));
    }

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>是否存在有限状态机。</returns>
    public bool HasFsm<T>(string name) where T : class
    {
        return InternalHasFsm(new TypeNamePair(typeof(T), name));
    }

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>是否存在有限状态机。</returns>
    public bool HasFsm(Type ownerType, string name)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalHasFsm(new TypeNamePair(ownerType, name));
    }

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <returns>要获取的有限状态机。</returns>
    public IFsm<T>? GetFsm<T>() where T : class
    {
        return InternalGetFsm(new TypeNamePair(typeof(T))) as IFsm<T>;
    }

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <returns>要获取的有限状态机。</returns>
    public FsmBase? GetFsm(Type ownerType)
    {
        return ownerType is null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalGetFsm(new TypeNamePair(ownerType));
    }

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>要获取的有限状态机。</returns>
    public IFsm<T>? GetFsm<T>(string name) where T : class
    {
        return InternalGetFsm(new TypeNamePair(typeof(T), name)) as IFsm<T>;
    }

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>要获取的有限状态机。</returns>
    public FsmBase? GetFsm(Type ownerType, string name)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalGetFsm(new TypeNamePair(ownerType, name));
    }

    /// <summary>
    ///     获取所有有限状态机。
    /// </summary>
    /// <returns>所有有限状态机。</returns>
    public FsmBase[] GetAllFsms()
    {
        return _fsms.Select(p => p.Value).ToArray();
    }

    /// <summary>
    ///     获取所有有限状态机。
    /// </summary>
    /// <param name="results">所有有限状态机。</param>
    public void GetAllFsms(out List<FsmBase> results)
    {
        results = _fsms.Select(fsm => fsm.Value).ToList();
    }

    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="states">有限状态机状态集合。</param>
    /// <returns>要创建的有限状态机。</returns>
    public IFsm<T> CreateFsm<T>(T owner, params FsmState<T>[] states) where T : class
    {
        return CreateFsm(string.Empty, owner, states);
    }

    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">有限状态机名称。</param>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="states">有限状态机状态集合。</param>
    /// <returns>要创建的有限状态机。</returns>
    public IFsm<T> CreateFsm<T>(string name, T owner, params FsmState<T>[] states) where T : class
    {
        var typeNamePair = new TypeNamePair(typeof(T), name);
        if (HasFsm<T>(name))
            throw new NetUtilityException(Utility.Text.Format("Already exist FSM '{0}'.", typeNamePair));

        var fsm = Fsm<T>.Create(name, owner, states);
        _fsms.Add(typeNamePair, fsm);
        return fsm;
    }

    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="states">有限状态机状态集合。</param>
    /// <returns>要创建的有限状态机。</returns>
    public IFsm<T> CreateFsm<T>(T owner, List<FsmState<T>> states) where T : class
    {
        return CreateFsm(string.Empty, owner, states);
    }

    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">有限状态机名称。</param>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="states">有限状态机状态集合。</param>
    /// <returns>要创建的有限状态机。</returns>
    public IFsm<T> CreateFsm<T>(string name, T owner, List<FsmState<T>> states) where T : class
    {
        var typeNamePair = new TypeNamePair(typeof(T), name);
        if (HasFsm<T>(name))
            throw new NetUtilityException(Utility.Text.Format("Already exist FSM '{0}'.", typeNamePair));

        var fsm = Fsm<T>.Create(name, owner, states);
        _fsms.Add(typeNamePair, fsm);
        return fsm;
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyFsm<T>() where T : class
    {
        return InternalDestroyFsm(new TypeNamePair(typeof(T)));
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyFsm(Type ownerType)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalDestroyFsm(new TypeNamePair(ownerType));
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">要销毁的有限状态机名称。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyFsm<T>(string name) where T : class
    {
        return InternalDestroyFsm(new TypeNamePair(typeof(T), name));
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <param name="name">要销毁的有限状态机名称。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyFsm(Type ownerType, string name)
    {
        return ownerType == null
            ? throw new NetUtilityException("Owner type is invalid.")
            : InternalDestroyFsm(new TypeNamePair(ownerType, name));
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="fsm">要销毁的有限状态机。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyFsm<T>(IFsm<T> fsm) where T : class
    {
        return fsm == null
            ? throw new NetUtilityException("FSM is invalid.")
            : InternalDestroyFsm(new TypeNamePair(typeof(T), fsm.Name));
    }

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <param name="fsm">要销毁的有限状态机。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    public bool DestroyFsm(FsmBase fsm)
    {
        return fsm == null
            ? throw new NetUtilityException("FSM is invalid.")
            : InternalDestroyFsm(new TypeNamePair(fsm.OwnerType, fsm.Name));
    }

    /// <summary>
    ///     有限状态机管理器轮询。
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    internal override void Update(float elapseSeconds, float realElapseSeconds)
    {
        _tempFsms.Clear();
        if (_fsms.Count <= 0) return;

        foreach (var fsm in _fsms) _tempFsms.Add(fsm.Value);

        foreach (var fsm in _tempFsms.Where(fsm => !fsm.IsDestroyed)) fsm.Update(elapseSeconds, realElapseSeconds);
    }

    /// <summary>
    ///     关闭并清理有限状态机管理器。
    /// </summary>
    internal override void Shutdown()
    {
        foreach (var fsm in _fsms) fsm.Value.Shutdown();

        _fsms.Clear();
        _tempFsms.Clear();
    }

    private bool InternalHasFsm(TypeNamePair typeNamePair)
    {
        return _fsms.ContainsKey(typeNamePair);
    }

    private FsmBase? InternalGetFsm(TypeNamePair typeNamePair)
    {
        return _fsms.GetValueOrDefault(typeNamePair);
    }

    private bool InternalDestroyFsm(TypeNamePair typeNamePair)
    {
        if (!_fsms.TryGetValue(typeNamePair, out var fsm)) return false;
        fsm.Shutdown();
        return _fsms.Remove(typeNamePair);
    }
}