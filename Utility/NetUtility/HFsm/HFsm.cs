using NetUtility.Module;

namespace NetUtility.HFsm;

internal sealed class HFsm<T> : HFsmBase, IReference, IHFsm<T> where T : class
{
    private readonly Dictionary<Type, HFsmState<T>> _states = new();
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
    public override int HFsmStateCount => _states.Count;

    /// <summary>
    ///     获取有限状态机持有者。
    /// </summary>
    public HFsmState<T>? RootState { get; private set; }

    /// <summary>
    ///     获取有限状态机是否正在运行。
    /// </summary>
    public override bool IsRunning => CurrentState is not null;

    /// <summary>
    ///     获取有限状态机是否被销毁。
    /// </summary>
    bool IHFsm<T>.IsDestroyed => IsDestroyed;

    /// <summary>
    ///     获取当前有限状态机状态。
    /// </summary>
    public HFsmState<T>? CurrentState { get; private set; }

    float IHFsm<T>.CurrentStateTime => CurrentStateTime;

    /// <summary>
    ///     开始有限状态机。
    /// </summary>
    public void Start()
    {
        if (IsRunning) throw new NetUtilityException("FSM is running, can not start again.");

        CurrentStateTime = 0f;
        CurrentState = RootState;
        RootState!.Enter(this);
    }

    /// <summary>
    ///     是否存在有限状态机状态。
    /// </summary>
    /// <typeparam name="TState">要检查的有限状态机状态类型。</typeparam>
    /// <returns>是否存在有限状态机状态。</returns>
    public bool HasState<TState>() where TState : HFsmState<T>
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

        return !typeof(HFsmState<T>).IsAssignableFrom(stateType)
            ? throw new NetUtilityException(Utility.Text.Format("State type '{0}' is invalid.", stateType.FullName))
            : _states.ContainsKey(stateType);
    }

    /// <summary>
    ///     获取有限状态机状态。
    /// </summary>
    /// <typeparam name="TState">要获取的有限状态机状态类型。</typeparam>
    /// <returns>要获取的有限状态机状态。</returns>
    public TState? GetState<TState>() where TState : HFsmState<T>
    {
        if (_states.TryGetValue(typeof(TState), out var state)) return (TState)state;

        return null;
    }

    /// <summary>
    ///     获取有限状态机状态。
    /// </summary>
    /// <param name="stateType">要获取的有限状态机状态类型。</param>
    /// <returns>要获取的有限状态机状态。</returns>
    public HFsmState<T>? GetState(Type stateType)
    {
        if (stateType == null) throw new NetUtilityException("State type is invalid.");

        return !typeof(HFsmState<T>).IsAssignableFrom(stateType)
            ? throw new NetUtilityException(Utility.Text.Format("State type '{0}' is invalid.", stateType.FullName))
            : _states.GetValueOrDefault(stateType);
    }

    /// <summary>
    ///     获取有限状态机的所有状态。
    /// </summary>
    /// <returns>有限状态机的所有状态。</returns>
    public HFsmState<T>[] GetAllStates()
    {
        return _states.Select(p => p.Value).ToArray();
    }

    /// <summary>
    ///     获取有限状态机的所有状态。
    /// </summary>
    /// <param name="results">有限状态机的所有状态。</param>
    public void GetAllStates(out List<HFsmState<T>> results)
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
        RootState?.Leave(this, true);

        foreach (var state in _states) state.Value.OnDestroy(this);

        Name = string.Empty;
        RootState = null;
        _states.Clear();

        if (_datas != null)
        {
            foreach (var data in _datas) ReferencePool.Release(data.Value);

            _datas.Clear();
        }

        CurrentStateTime = 0f;
        IsDestroyed = true;
    }

    internal override void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (!IsRunning) return;
        CurrentStateTime += elapseSeconds;
        RootState!.Update(this, elapseSeconds, realElapseSeconds);
    }

    /// <summary>
    ///     关闭并清理有限状态机。
    /// </summary>
    internal override void Shutdown()
    {
        ChangeState(RootState!);
        ReferencePool.Release(this);
    }

    public static HFsm<T> Create(string name, T owner, HFsmState<T> rootState)
    {
        var hfsm = ReferencePool.Acquire<HFsm<T>>();
        hfsm.Name = name;
        hfsm.Owner = owner;
        hfsm.RootState = rootState;
        hfsm.IsDestroyed = false;
        Wire(rootState, hfsm);
        return hfsm;
    }

    public static HFsm<T> Create<TRoot>(string name, T owner) where TRoot : HFsmState<T>, new()
    {
        var rootState = new TRoot();
        var hfsm = ReferencePool.Acquire<HFsm<T>>();
        hfsm.Name = name;
        hfsm.Owner = owner;
        hfsm.RootState = rootState;
        hfsm.IsDestroyed = false;
        Wire(rootState, hfsm);
        return hfsm;
    }

    /// <summary>
    ///     切换当前有限状态机状态。
    /// </summary>
    /// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
    internal void ChangeState<TState>() where TState : HFsmState<T>
    {
        ChangeState(typeof(TState));
    }

    /// <summary>
    ///     切换当前有限状态机状态。
    /// </summary>
    /// <param name="stateType">要切换到的有限状态机状态类型。</param>
    internal void ChangeState(Type stateType)
    {
        ChangeState(GetState(stateType));
    }

    /// <summary>
    ///     切换当前有限状态机状态。
    /// </summary>
    /// <param name="to">要切换到的有限状态机状态。</param>
    internal void ChangeState(HFsmState<T>? to)
    {
        var from = CurrentState;
        if (to is null)
            throw new NetUtilityException(Utility.Text.Format("HFSM '{0}' can not change state to a null state.",
                new TypeNamePair(typeof(T), Name)));

        CurrentStateTime = 0f;
        if (from == to || from is null) return;
        var lca = Lca(from, to);
        for (var s = from; s != lca; s = s.Parent!) s.Leave(this, false);
        var stack = new Stack<HFsmState<T>>();
        for (var s = to; s != lca; s = s.Parent!) stack.Push(s);
        while (stack.Count > 0) stack.Pop().Enter(this);
    }


    /// <summary>
    ///     检测验证状态机
    /// </summary>
    /// <param name="state">当前检测节点</param>
    /// <param name="hfsm">状态机管理器</param>
    private static void Wire(HFsmState<T>? state, HFsm<T> hfsm)
    {
        //空状态不处理
        if (state is null) return;
        //已经处理过的状态不再处理
        if (hfsm._states.ContainsKey(state.GetType())) return;
        hfsm._states[state.GetType()] = state;
        state.OnInit(hfsm);
        //递归注册子状态
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.FlattenHierarchy;
        foreach (var childFiledInfo in state.GetType().GetFields(flags))
        {
            //不是状态类不处理
            if (!typeof(HFsmState<T>).IsAssignableFrom(childFiledInfo.FieldType)) continue;
            //父状态不处理
            if (childFiledInfo.Name == nameof(HFsmState<T>.Parent)) continue;
            //子状态空值不处理
            if (childFiledInfo.GetValue(state) is not HFsmState<T> child) continue;
            //子状态的父状态不是当前状态不处理
            if (!ReferenceEquals(child.Parent, state)) continue;
            //递归检测子节点
            Wire(child, hfsm);
        }
    }


    public static HFsmState<T>? Lca(HFsmState<T>? a, HFsmState<T>? b)
    {
        var ap = new HashSet<HFsmState<T>>();
        for (var s = a; s is not null; s = s.Parent) ap.Add(s);
        for (var s = b; s is not null; s = s.Parent)
            if (ap.Contains(s))
                return s;

        return null;
    }
}