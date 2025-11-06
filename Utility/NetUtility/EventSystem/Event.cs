namespace NetUtility.EventSystem;

//事件对象基类
public abstract class Event
{
    public bool IsCanceled { get; protected internal set; }
}

//可取消的事件接口
public interface ICancellableEvent
{
    bool IsCanceled { get; }
    void SetCanceled(bool isCanceled);
}

public abstract class CancellableEvent : Event, ICancellableEvent
{
    public void SetCanceled(bool isCanceled)
    {
        IsCanceled = isCanceled;
    }
}