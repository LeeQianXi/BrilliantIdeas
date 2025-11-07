namespace NetUtility.Fsm;

/// <summary>
///     有限状态机。
/// </summary>
/// <typeparam name="T">有限状态机持有者类型。</typeparam>
internal sealed class Fsm<T> : FsmBase, IReference, IFsm<T> where T : class
{
    private readonly Dictionary<Type, FsmState<T>> _states = new();
    private Dictionary<string, Variable>? _datas;

    /// <summary>
    ///     获取有限状态机持有者类型。
    /// </summary>
    public override Type OwnerType => typeof(T);

    /// <summary>
    ///     获取有限状态机是否被销毁。
    /// </summary>
    public override bool IsDestroyed { get; protected set; } = true;

    /// <summary>
    ///     获取当前有限状态机状态名称。
    /// </summary>
    public override string? CurrentStateName => CurrentState?.GetType().FullName;

    /// <summary>
    ///     获取当前有限状态机状态持续时间。
    /// </summary>
    public override float CurrentStateTime { get; protected set; }

    /// <summary>
    ///     获取有限状态机持有者。
    /// </summary>
    public T? Owner { get; private set; }

    /// <summary>
    ///     获取有限状态机中状态的数量。
    /// </summary>
    public override int FsmStateCount => _states.Count;

    /// <summary>
    ///     获取有限状态机是否正在运行。
    /// </summary>
    public override bool IsRunning => CurrentState is not null;

    /// <summary>
    ///     获取有限状态机是否被销毁。
    /// </summary>
    bool IFsm<T>.IsDestroyed => IsDestroyed;

    /// <summary>
    ///     获取当前有限状态机状态。
    /// </summary>
    public FsmState<T>? CurrentState { get; private set; }

    float IFsm<T>.CurrentStateTime => CurrentStateTime;

    /// <summary>
    ///     开始有限状态机。
    /// </summary>
    /// <typeparam name="TState">要开始的有限状态机状态类型。</typeparam>
    public void Start<TState>() where TState : FsmState<T>
    {
        if (IsRunning) throw new NetUtilityException("FSM is running, can not start again.");

        var state = GetState<TState>();

        CurrentStateTime = 0f;
        CurrentState = state ?? throw new NetUtilityException(Utility.Text.Format(
            "FSM '{0}' can not start state '{1}' which is not exist.", new TypeNamePair(typeof(T), Name),
            typeof(TState).FullName));
        CurrentState.OnEnter(this);
    }

    /// <summary>
    ///     开始有限状态机。
    /// </summary>
    /// <param name="stateType">要开始的有限状态机状态类型。</param>
    public void Start(Type stateType)
    {
        if (IsRunning) throw new NetUtilityException("FSM is running, can not start again.");

        if (stateType == null) throw new NetUtilityException("State type is invalid.");

        if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            throw new NetUtilityException(Utility.Text.Format("State type '{0}' is invalid.", stateType.FullName));

        var state = GetState(stateType);

        CurrentStateTime = 0f;
        CurrentState = state ?? throw new NetUtilityException(Utility.Text.Format(
            "FSM '{0}' can not start state '{1}' which is not exist.", new TypeNamePair(typeof(T), Name),
            stateType.FullName));
        CurrentState.OnEnter(this);
    }

    /// <summary>
    ///     是否存在有限状态机状态。
    /// </summary>
    /// <typeparam name="TState">要检查的有限状态机状态类型。</typeparam>
    /// <returns>是否存在有限状态机状态。</returns>
    public bool HasState<TState>() where TState : FsmState<T>
    {
        return _states.ContainsKey(typeof(TState));
    }

    /// <summary>
    ///     是否存在有限状态机状态。
    /// </summary>
    /// <param name="stateType">要检查的有限状态机状态类型。</param>
    /// <returns>是否存在有限状态机状态。</returns>
    public bool HasState(Type stateType)
    {
        if (stateType == null) throw new NetUtilityException("State type is invalid.");

        return !typeof(FsmState<T>).IsAssignableFrom(stateType)
            ? throw new NetUtilityException(Utility.Text.Format("State type '{0}' is invalid.", stateType.FullName))
            : _states.ContainsKey(stateType);
    }

    /// <summary>
    ///     获取有限状态机状态。
    /// </summary>
    /// <typeparam name="TState">要获取的有限状态机状态类型。</typeparam>
    /// <returns>要获取的有限状态机状态。</returns>
    public TState? GetState<TState>() where TState : FsmState<T>
    {
        if (_states.TryGetValue(typeof(TState), out var state)) return (TState)state;

        return null;
    }

    /// <summary>
    ///     获取有限状态机状态。
    /// </summary>
    /// <param name="stateType">要获取的有限状态机状态类型。</param>
    /// <returns>要获取的有限状态机状态。</returns>
    public FsmState<T>? GetState(Type stateType)
    {
        if (stateType == null) throw new NetUtilityException("State type is invalid.");

        return !typeof(FsmState<T>).IsAssignableFrom(stateType)
            ? throw new NetUtilityException(Utility.Text.Format("State type '{0}' is invalid.", stateType.FullName))
            : _states.GetValueOrDefault(stateType);
    }

    /// <summary>
    ///     获取有限状态机的所有状态。
    /// </summary>
    /// <returns>有限状态机的所有状态。</returns>
    public FsmState<T>[] GetAllStates()
    {
        return _states.Select(p => p.Value).ToArray();
    }

    /// <summary>
    ///     获取有限状态机的所有状态。
    /// </summary>
    /// <param name="results">有限状态机的所有状态。</param>
    public void GetAllStates(out List<FsmState<T>> results)
    {
        results = _states.Select(p => p.Value).ToList();
    }

    /// <summary>
    ///     是否存在有限状态机数据。
    /// </summary>
    /// <param name="name">有限状态机数据名称。</param>
    /// <returns>有限状态机数据是否存在。</returns>
    public bool HasData(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new NetUtilityException("Data name is invalid.");

        return _datas != null && _datas.ContainsKey(name);
    }

    /// <summary>
    ///     获取有限状态机数据。
    /// </summary>
    /// <typeparam name="TData">要获取的有限状态机数据的类型。</typeparam>
    /// <param name="name">有限状态机数据名称。</param>
    /// <returns>要获取的有限状态机数据。</returns>
    public TData? GetData<TData>(string name) where TData : Variable
    {
        return GetData(name) as TData;
    }

    /// <summary>
    ///     获取有限状态机数据。
    /// </summary>
    /// <param name="name">有限状态机数据名称。</param>
    /// <returns>要获取的有限状态机数据。</returns>
    public Variable? GetData(string name)
    {
        return string.IsNullOrEmpty(name)
            ? throw new NetUtilityException("Data name is invalid.")
            : _datas?.GetValueOrDefault(name);
    }

    /// <summary>
    ///     设置有限状态机数据。
    /// </summary>
    /// <typeparam name="TData">要设置的有限状态机数据的类型。</typeparam>
    /// <param name="name">有限状态机数据名称。</param>
    /// <param name="data">要设置的有限状态机数据。</param>
    public void SetData<TData>(string name, TData data) where TData : Variable
    {
        SetData(name, (Variable)data);
    }

    /// <summary>
    ///     设置有限状态机数据。
    /// </summary>
    /// <param name="name">有限状态机数据名称。</param>
    /// <param name="data">要设置的有限状态机数据。</param>
    public void SetData(string name, Variable data)
    {
        if (string.IsNullOrEmpty(name)) throw new NetUtilityException("Data name is invalid.");

        _datas ??= new Dictionary<string, Variable>(StringComparer.Ordinal);

        var oldData = GetData(name);
        if (oldData != null) ReferencePool.Release(oldData);

        _datas[name] = data;
    }

    /// <summary>
    ///     移除有限状态机数据。
    /// </summary>
    /// <param name="name">有限状态机数据名称。</param>
    /// <returns>是否移除有限状态机数据成功。</returns>
    public bool RemoveData(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new NetUtilityException("Data name is invalid.");

        if (_datas == null) return false;

        var oldData = GetData(name);
        if (oldData != null) ReferencePool.Release(oldData);

        return _datas.Remove(name);
    }

    /// <summary>
    ///     清理有限状态机。
    /// </summary>
    public void Reset()
    {
        CurrentState?.OnLeave(this, true);

        foreach (var state in _states) state.Value.OnDestroy(this);

        Name = string.Empty;
        Owner = null;
        _states.Clear();

        if (_datas != null)
        {
            foreach (var data in _datas) ReferencePool.Release(data.Value);

            _datas.Clear();
        }

        CurrentState = null;
        CurrentStateTime = 0f;
        IsDestroyed = true;
    }

    internal override void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (CurrentState == null) return;

        CurrentStateTime += elapseSeconds;
        CurrentState.OnUpdate(this, elapseSeconds, realElapseSeconds);
    }

    /// <summary>
    ///     关闭并清理有限状态机。
    /// </summary>
    internal override void Shutdown()
    {
        ReferencePool.Release(this);
    }

    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <param name="name">有限状态机名称。</param>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="states">有限状态机状态集合。</param>
    /// <returns>创建的有限状态机。</returns>
    public static Fsm<T> Create(string name, T owner, params FsmState<T>[] states)
    {
        if (owner == null) throw new NetUtilityException("FSM owner is invalid.");

        if (states == null || states.Length < 1) throw new NetUtilityException("FSM states is invalid.");

        var fsm = ReferencePool.Acquire<Fsm<T>>();
        fsm.Name = name;
        fsm.Owner = owner;
        fsm.IsDestroyed = false;
        foreach (var state in states)
        {
            if (state == null) throw new NetUtilityException("FSM states is invalid.");

            var stateType = state.GetType();
            if (!fsm._states.TryAdd(stateType, state))
                throw new NetUtilityException(Utility.Text.Format("FSM '{0}' state '{1}' is already exist.",
                    new TypeNamePair(typeof(T), name), stateType.FullName));

            state.OnInit(fsm);
        }

        return fsm;
    }

    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <param name="name">有限状态机名称。</param>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="states">有限状态机状态集合。</param>
    /// <returns>创建的有限状态机。</returns>
    public static Fsm<T> Create(string name, T owner, List<FsmState<T>> states)
    {
        if (owner == null) throw new NetUtilityException("FSM owner is invalid.");

        if (states == null || states.Count < 1) throw new NetUtilityException("FSM states is invalid.");

        var fsm = ReferencePool.Acquire<Fsm<T>>();
        fsm.Name = name;
        fsm.Owner = owner;
        fsm.IsDestroyed = false;
        foreach (var state in states)
        {
            if (state == null) throw new NetUtilityException("FSM states is invalid.");

            var stateType = state.GetType();
            if (!fsm._states.TryAdd(stateType, state))
                throw new NetUtilityException(Utility.Text.Format("FSM '{0}' state '{1}' is already exist.",
                    new TypeNamePair(typeof(T), name), stateType.FullName));

            state.OnInit(fsm);
        }

        return fsm;
    }

    /// <summary>
    ///     有限状态机轮询。
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    /// <summary>
    ///     切换当前有限状态机状态。
    /// </summary>
    /// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
    internal void ChangeState<TState>() where TState : FsmState<T>
    {
        ChangeState(typeof(TState));
    }

    /// <summary>
    ///     切换当前有限状态机状态。
    /// </summary>
    /// <param name="stateType">要切换到的有限状态机状态类型。</param>
    internal void ChangeState(Type stateType)
    {
        if (CurrentState == null) throw new NetUtilityException("Current state is invalid.");

        var state = GetState(stateType);
        if (state == null)
            throw new NetUtilityException(Utility.Text.Format(
                "FSM '{0}' can not change state to '{1}' which is not exist.", new TypeNamePair(typeof(T), Name),
                stateType.FullName));

        CurrentState.OnLeave(this, false);
        CurrentStateTime = 0f;
        CurrentState = state;
        CurrentState.OnEnter(this);
    }
}