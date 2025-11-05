namespace NetUtility.EventSystem;

public interface IEventBus
{
    /// <summary>
    ///     注册指定对象所有带有<see cref="SubscribeEventAttribute" />的成员函数
    /// </summary>
    void Register(object target);

    /// <summary>
    ///     注册指定类所有带有<see cref="SubscribeEventAttribute" />的静态函数
    /// </summary>
    void Register(Type target);

    void AddListener<T>(Action<T> listener)
        where T : Event;

    void AddListener<T>(EventPriority priority, Action<T> listener)
        where T : Event;

    void AddListener<T>(bool receiveCanceled, Action<T> listener)
        where T : Event;

    void AddListener<T>(EventPriority priority, bool receiveCanceled, Action<T> listener)
        where T : Event;

    void UnRegister(object target);

    T PostEvent<T>(T @event)
        where T : Event;

    T PostEvent<T>(EventPriority phase, T @event)
        where T : Event;

    T PostEventNow<T>(T @event)
        where T : Event;

    T PostEventNow<T>(EventPriority phase, T @event)
        where T : Event;

    void Start();
    void Stop();
}

public interface IEventExceptionHandler
{
    void HandleException(IEventBus bus, Event @event, IEventListener[] listeners, int index, Exception exception);
}

public delegate void EventClassChecker(Type eventType);