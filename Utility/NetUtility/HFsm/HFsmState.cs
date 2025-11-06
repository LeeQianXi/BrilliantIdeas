using NetUtility.Module;

namespace NetUtility.HFsm;

public abstract class HFsmState<T>(HFsmState<T>? parent = null)
    where T : class
{
    public readonly HFsmState<T>? Parent = parent;
    public HFsmState<T>? ActiveChild;

    protected internal virtual HFsmState<T>? GetInitialState()
    {
        return null;
    }

    protected internal virtual HFsmState<T>? GetTransitionState()
    {
        return null;
    }

    //TODO: LifeCycle methods
    /// <summary>
    ///     有限状态机状态初始化时调用。
    /// </summary>
    /// <param name="hfsm">有限状态机引用。</param>
    protected internal virtual void OnInit(IHFsm<T> hfsm)
    {
    }

    /// <summary>
    ///     有限状态机状态进入时调用。
    /// </summary>
    /// <param name="hfsm">有限状态机引用。</param>
    protected internal virtual void OnEnter(IHFsm<T> hfsm)
    {
    }

    /// <summary>
    ///     有限状态机状态轮询时调用。
    /// </summary>
    /// <param name="hfsm">有限状态机引用。</param>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    protected internal virtual void OnUpdate(IHFsm<T> hfsm, float elapseSeconds, float realElapseSeconds)
    {
    }

    /// <summary>
    ///     有限状态机状态离开时调用。
    /// </summary>
    /// <param name="hfsm">有限状态机引用。</param>
    /// <param name="isShutdown">是否是关闭有限状态机时触发。</param>
    protected internal virtual void OnLeave(IHFsm<T> hfsm, bool isShutdown)
    {
    }

    /// <summary>
    ///     有限状态机状态销毁时调用。
    /// </summary>
    /// <param name="hfsm">有限状态机引用。</param>
    protected internal virtual void OnDestroy(IHFsm<T> hfsm)
    {
    }

    //TODO: Internal methods
    internal void Enter(IHFsm<T> hfsm)
    {
        if (Parent is not null) Parent.ActiveChild = this;

        OnEnter(hfsm);
        var init = GetInitialState();
        if (init is not null) init.Enter(hfsm);
    }

    internal void Leave(IHFsm<T> hfsm, bool isShutdown)
    {
        if (ActiveChild is not null) ActiveChild.Leave(hfsm, isShutdown);

        ActiveChild = null;

        OnLeave(hfsm, isShutdown);
        if (Parent is not null)
            Parent.ActiveChild = null;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    internal void Update(IHFsm<T> hfsm, float elapseSeconds, float realElapseSeconds)
    {
        var t = GetTransitionState();
        if (t is not null)
        {
            ChangeState(hfsm, t);
            return;
        }

        ActiveChild?.Update(hfsm, elapseSeconds, realElapseSeconds);

        OnUpdate(hfsm, elapseSeconds, realElapseSeconds);
    }

    //TODO: Tools methods
    public HFsmState<T> Leaf()
    {
        var s = this;
        while (s.ActiveChild is not null) s = s.ActiveChild;

        return s;
    }

    public IEnumerable<HFsmState<T>> PathToRoot()
    {
        for (var s = this; s is not null; s = s.Parent) yield return s;
    }

    private void ChangeState(IHFsm<T> hfsm, HFsmState<T> to)
    {
        var hfsmImpl = (HFsm<T>)hfsm;
        if (hfsmImpl == null) throw new NetUtilityException("HFSM is invalid.");
        if (!hfsmImpl.HasState(to.GetType())) throw new NetUtilityException("State is invalid.");
        hfsmImpl.ChangeState(to);
    }

    protected void ChangeState<TState>(IHFsm<T> hfsm) where TState : HFsmState<T>
    {
        var hfsmImpl = (HFsm<T>)hfsm;
        if (hfsmImpl == null) throw new NetUtilityException("HFSM is invalid.");
        hfsmImpl.ChangeState<TState>();
    }

    protected void ChangeState(IHFsm<T> hfsm, Type stateType)
    {
        var hfsmImpl = (HFsm<T>)hfsm;
        if (hfsmImpl == null) throw new NetUtilityException("HFSM is invalid.");
        if (stateType == null) throw new NetUtilityException("State type is invalid.");
        if (!typeof(HFsmState<T>).IsAssignableFrom(stateType))
            throw new NetUtilityException(Utility.Text.Format("State type '{0}' is invalid.", stateType.FullName));
        hfsmImpl.ChangeState(stateType);
    }
}