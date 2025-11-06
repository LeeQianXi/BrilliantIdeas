namespace NetUtility.EventSystem;

/// 订阅注解属性
[AttributeUsage(AttributeTargets.Method)]
public class SubscribeEventAttribute(
    EventPriority priority = EventPriority.Normal,
    bool receiveCanceled = false)
    : Attribute
{
    public EventPriority Priority { get; } = priority;
    public bool ReceiveCanceled { get; } = receiveCanceled;
}