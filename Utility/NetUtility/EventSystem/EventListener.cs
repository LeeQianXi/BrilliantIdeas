namespace NetUtility.EventSystem;

public interface IEventListener
{
    void Invoke(Event @event);

    internal class EventListenerImpl(Action<Event> handler) : IEventListener
    {
        public void Invoke(Event @event)
        {
            handler(@event);
        }
    }
}

static partial class EventExtensions
{
    extension(Action<Event> handler)
    {
        public IEventListener Wrapper()
        {
            return new IEventListener.EventListenerImpl(handler);
        }
    }
}

public interface IWrapperListener
{
    IEventListener GetWithoutCheck();
}

internal class SubscribeEventListener : IEventListener
{
    private readonly MethodInfo _method;
    private readonly object _target;

    public SubscribeEventListener(object target, MethodInfo method)
    {
        _target = target;
        _method = method;
        var attr = method.GetCustomAttribute<SubscribeEventAttribute>();
        Priority = attr?.Priority ?? EventPriority.Normal;
    }

    public EventPriority Priority { get; }

    public void Invoke(Event @event)
    {
        _method.Invoke(_target is Type ? null : _target, [@event]);
    }
}

public class ConsumerEventHandler<T>(Action<T> consumer) : IEventListener
{
    private readonly Action<T> _consumer = consumer;

    public void Invoke(Event @event)
    {
        if (@event is T t)
            _consumer.Invoke(t);
    }

    public override string? ToString()
    {
        return _consumer.ToString();
    }

    public sealed class WithPredicate(Action<T> consumer, Predicate<T> predicate)
        : ConsumerEventHandler<T>(consumer), IWrapperListener, IEventListener
    {
        private readonly IEventListener _withoutCheck = new ConsumerEventHandler<T>(consumer);

        public new void Invoke(Event @event)
        {
            if (@event is T t && predicate(t))
                _consumer.Invoke(t);
        }

        public IEventListener GetWithoutCheck()
        {
            return _withoutCheck;
        }
    }
}