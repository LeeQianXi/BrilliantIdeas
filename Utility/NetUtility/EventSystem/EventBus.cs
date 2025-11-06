namespace NetUtility.EventSystem;

internal sealed partial class EventBus : IEventBus, IEventExceptionHandler
{
    private static readonly bool CheckTypesOnDispatchProperty =
        bool.Parse(Environment.GetEnvironmentVariable("eventbus.checkTypesOnDispatch") ?? "false");

    // 是否允许在不同阶段分派事件
    private readonly bool _allowPerPhasePost;

    // 是否在分派事件时检查事件类型
    private readonly bool _checkTypesOnDispatch;

    // 事件类型检测器
    private readonly EventClassChecker _classChecker;

    // 事件错误处理器
    private readonly IEventExceptionHandler _exceptionHandler;

    // Listener实际存储
    private readonly ConcurrentDictionary<Type, ListenerList> _listenerLists = new();

    // Listener 表层封装存储
    private readonly ConcurrentDictionary<object, List<IEventListener>> _listeners = new();

    // 事件总线是否已关闭
    private volatile bool _shutdown;

    private EventBus(IEventExceptionHandler? handler, bool startShutdown, EventClassChecker classChecker,
        bool checkTypesOnDispatch, bool allowPerPhasePost)
    {
        _exceptionHandler = handler ?? this;
        _shutdown = startShutdown;
        _classChecker = classChecker;
        _checkTypesOnDispatch = checkTypesOnDispatch || CheckTypesOnDispatchProperty;
        _allowPerPhasePost = allowPerPhasePost;
    }

    public EventBus(IBusBuilder busBuilder)
        : this(busBuilder.ExceptionHandler, busBuilder.IsStartShutdown, busBuilder.ClassChecker,
            busBuilder.IsCheckTypesOnDispatch, busBuilder.IsAllowPerPhasePost)
    {
    }

    public void Register(object target)
    {
        if (_listeners.ContainsKey(target)) return;
        if (target is Type subscribeType)
        {
            Register(subscribeType);
            return;
        }

        var type = target.GetType();

        CheckSupertypes(type, type);

        var foundMethods = 0;
        foreach (var methodInfo in type.GetDeclaredMethods())
        {
            if (!methodInfo.HasAttribute<SubscribeEventAttribute>())
                continue;

            if (!methodInfo.IsStatic)
                RegisterListener(target, methodInfo);
            else
                throw new ArgumentException(
                    $"预期的SubscribeEventAttribute方法{methodInfo.Name}是静态的,因为Register方法是由类实例对象调用的，要么将该方法设置为非静态，要么使用{type.Name}的Type对象调用Register().");

            ++foundMethods;
        }

        if (foundMethods is 0)
            throw new ArgumentException(
                $"{type.Name}没有SubscribeEventAttribute方法，但Register仍被调用。\n事件总线仅识别具有SubscribeEventAttribute特性的侦听器方法。");
    }

    public void Register(Type type)
    {
        CheckSupertypes(type, type);

        var foundMethods = 0;
        foreach (var methodInfo in type.GetDeclaredMethods())
        {
            if (!methodInfo.HasAttribute<SubscribeEventAttribute>())
                continue;

            if (methodInfo.IsStatic)
                RegisterListener(type, methodInfo);
            else
                throw new ArgumentException(
                    $"预期的SubscribeEventAttribute方法{methodInfo.Name}不是静态的,因为Register方法是由类Type对象调用的，要么将该方法设置为静态，要么使用{type.Name}的实例调用Register().");

            ++foundMethods;
        }

        if (foundMethods is 0)
            throw new ArgumentException(
                $"{type.Name}没有SubscribeEventAttribute方法，但Register仍被调用。\n事件总线仅识别具有SubscribeEventAttribute特性的侦听器方法。");
    }

    public void AddListener<T>(Action<T> listener) where T : Event
    {
        AddListener(EventPriority.Normal, false, listener);
    }

    public void AddListener<T>(EventPriority priority, Action<T> listener) where T : Event
    {
        AddListener(priority, false, listener);
    }

    public void AddListener<T>(bool receiveCanceled, Action<T> listener) where T : Event
    {
        AddListener(EventPriority.Normal, receiveCanceled, listener);
    }

    public void AddListener<T>(EventPriority priority, bool receiveCanceled, Action<T> listener) where T : Event
    {
        AddListener(priority, PassNotGenericFilter<T>(receiveCanceled), listener);
    }

    public void UnRegister(object target)
    {
        _listeners.TryRemove(target, out var list);
        if (list is null) return;
        foreach (var listenerList in _listenerLists.Values.AsReadOnlyCollection())
        foreach (var listener in list)
            listenerList.Unregister(listener);
    }

    public T PostEvent<T>(T @event) where T : Event
    {
        if (_shutdown)
            return @event;
        DoPostChecks(@event);
        return PostEvent(@event, GetListenerList(typeof(T)).GetListeners());
    }

    public T PostEvent<T>(EventPriority phase, T @event) where T : Event
    {
        if (!_allowPerPhasePost)
            throw new ArgumentException("该事件总线不允许呼叫阶段限定的事件分派.");
        if (_shutdown)
            return @event;
        DoPostChecks(@event);
        return PostEvent(@event, GetListenerList(typeof(T)).GetPhaseListeners(phase));
    }

    public T PostEventNow<T>(T @event) where T : Event
    {
        if (_shutdown)
            return @event;
        DoPostChecks(@event);
        return PostEventNow(@event, GetListenerList(typeof(T)).GetListeners());
    }

    public T PostEventNow<T>(EventPriority phase, T @event) where T : Event
    {
        if (!_allowPerPhasePost)
            throw new ArgumentException("该事件总线不允许呼叫阶段限定的事件分派.");
        if (_shutdown)
            return @event;
        DoPostChecks(@event);
        return PostEventNow(@event, GetListenerList(typeof(T)).GetPhaseListeners(phase));
    }

    public void Start()
    {
        _shutdown = false;
    }

    public void Stop()
    {
        _shutdown = true;
    }

    public void HandleException(IEventBus bus, Event @event, IEventListener[] listeners, int index,
        Exception exception)
    {
        Console.WriteLine($"事件总线{bus}在分派事件{@event}时发生异常，在第{index + 1}个侦听器{listeners[index]}上。");
    }

    private static void CheckSupertypes(Type requestedType, Type? type)
    {
        if (type is null || type == typeof(object)) return;
        if (type != requestedType)
            foreach (var methodInfo in type.GetDeclaredMethods())
                if (methodInfo.HasAttribute<SubscribeEventAttribute>())
                    throw new ArgumentException(
                        $"正在尝试注册{requestedType.Name}类型的侦听器对象，\n但是，它的超类型{type.Name}有一个@SubscribeEvent方法:{methodInfo.Name}。\n这是不允许的！只有侦听器对象可以具有SubscribeEventAttribute方法，而不能有超类型具有SubscribeEventAttribute方法。");

        CheckSupertypes(requestedType, type.BaseType);
        foreach (var @interface in type.GetInterfaces()) CheckSupertypes(requestedType, @interface);
    }

    private void RegisterListener(object target, MethodInfo methodInfo)
    {
        var parameters = methodInfo.GetParameters();
        if (parameters.Length != 1)
            throw new ArgumentException(
                $"预期的SubscribeEventAttribute方法{methodInfo.Name}必须有且仅有一个参数，但实际上有{parameters.Length}个参数。");

        var eventType = parameters[0].ParameterType;
        if (!typeof(Event).IsAssignableFrom(eventType))
            throw new ArgumentException(
                $"预期的SubscribeEventAttribute方法{methodInfo.Name}的第一个参数必须是Event的子类，但实际上是{eventType.Name}。");

        try
        {
            _classChecker.Invoke(eventType);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"预期的SubscribeEventAttribute方法{methodInfo.Name}的第一个参数不是该总线所允许的事件类型。", ex);
        }

        Register(eventType, target, methodInfo);
    }

    private static Predicate<T>? PassNotGenericFilter<T>(bool receiveCanceled) where T : Event
    {
        // The cast is safe because the filter is removed if the event is not cancellable
        return receiveCanceled ? null : e => !((ICancellableEvent)e).IsCanceled;
    }

    private void AddListener<T>(EventPriority property, Predicate<T>? filter, Action<T> consumer) where T : Event
    {
        try
        {
            _classChecker.Invoke(typeof(T));
        }
        catch (ArgumentException e)
        {
            throw new ArgumentException($"event {typeof(T).Name} 的侦听器接收了一个改总线不支持的参数类型.", e);
        }

        IEventListener listener = filter is null
            ? new ConsumerEventHandler<T>(consumer)
            : new ConsumerEventHandler<T>.WithPredicate(consumer, filter);
        AddToListeners(consumer, typeof(T), listener, property);
    }

    private void Register(Type eventType, object target, MethodInfo method)
    {
        var listener = new SubscribeEventListener(target, method);
        AddToListeners(target, eventType, listener, listener.Priority);
    }

    private void AddToListeners(object target, Type eventType, IEventListener listener, EventPriority priority)
    {
        if (eventType.IsAbstract) throw new ArgumentException($"抽象事件{eventType.Name}不能被注册.");

        GetListenerList(eventType).Register(priority, listener);
        var others = _listeners.GetOrAdd(target, _ => []);
        others.Add(listener);
    }

    private ListenerList GetListenerList(Type eventType)
    {
        var list = _listenerLists.GetValueOrDefault(eventType);
        if (list is not null) return list;
        if (!eventType.BaseType!.IsAbstract)
        {
            return _listenerLists.GetOrAdd(eventType, ValueFactory1);

            ListenerList ValueFactory1(Type type)
            {
                return new ListenerList(type, GetListenerList(eventType.BaseType), _allowPerPhasePost);
            }
        }

        ValidateAbstractChain(eventType.BaseType);
        return _listenerLists.GetOrAdd(eventType, ValueFactory2);

        ListenerList ValueFactory2(Type type)
        {
            return new ListenerList(type, _allowPerPhasePost);
        }
    }

    private static void ValidateAbstractChain(Type eventType)
    {
        while (eventType != typeof(Event))
        {
            if (!eventType.IsAbstract)
                throw new ArgumentException($"抽象事件{eventType.Name}有一个非抽象的超类{eventType.BaseType!.Name},超类必须是抽象的.");

            eventType = eventType.BaseType!;
        }
    }

    private void DoPostChecks(Event @event)
    {
        if (!_checkTypesOnDispatch) return;
        try
        {
            _classChecker.Invoke(@event.GetType());
        }
        catch (ArgumentException e)
        {
            throw new ArgumentException($"禁止在该事件总线分派一个不支持的事件类型{@event.GetType().Name}", e);
        }
    }

    private T PostEvent<T>(T @event, IEventListener[] listeners) where T : Event
    {
        return PostEventNow(@event, listeners);
    }

    private T PostEventNow<T>(T @event, IEventListener[] listeners) where T : Event
    {
        var index = 0;
        try
        {
            for (; index < listeners.Length; index++) listeners[index].Invoke(@event);
        }
        catch (Exception e)
        {
            _exceptionHandler.HandleException(this, @event, listeners, index, e);
            throw;
        }

        return @event;
    }
}