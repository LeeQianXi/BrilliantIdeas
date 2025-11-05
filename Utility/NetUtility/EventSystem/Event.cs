namespace NetUtility.EventSystem;

//事件对象基类
public abstract class Event
{
    public bool IsCanceled { get; protected internal set; } = false;
}

//可取消的事件接口
public interface ICancellableEvent
{
    void SetCanceled(bool isCanceled);
    bool IsCanceled();
}