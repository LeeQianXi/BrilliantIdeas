namespace NetUtility.EventSystem;

public interface IBusBuilder
{
    public IEventExceptionHandler? ExceptionHandler { get; }
    public bool IsStartShutdown { get; }
    public bool IsCheckTypesOnDispatch { get; }
    public EventClassChecker ClassChecker { get; }
    public bool IsAllowPerPhasePost { get; }
    IBusBuilder SetExceptionHandler(IEventExceptionHandler exceptionHandler);
    IBusBuilder SetIsStartShutdown(bool isStartShutdown);
    IBusBuilder SetCheckTypesOnDispatch(bool isCheckTypesOnDispatch);
    IBusBuilder MarkerType(Type markerInterface);
    IBusBuilder SetClassChecker(EventClassChecker classChecker);
    IBusBuilder SetAllowPerPhasePost(bool isAllowPerPhasePost);
    IEventBus Build();

    public static IBusBuilder Builder()
    {
        return new BusBuilderImpl();
    }

    private sealed class BusBuilderImpl : IBusBuilder
    {
        public IEventExceptionHandler? ExceptionHandler { get; private set; }
        public bool IsStartShutdown { get; private set; }
        public bool IsCheckTypesOnDispatch { get; private set; }
        public EventClassChecker ClassChecker { get; private set; } = _ => { };
        public bool IsAllowPerPhasePost { get; private set; }

        public IBusBuilder SetExceptionHandler(IEventExceptionHandler exceptionHandler)
        {
            ExceptionHandler = exceptionHandler;
            return this;
        }

        public IBusBuilder SetIsStartShutdown(bool isStartShutdown)
        {
            IsStartShutdown = isStartShutdown;
            return this;
        }

        public IBusBuilder SetCheckTypesOnDispatch(bool isCheckTypesOnDispatch)
        {
            IsCheckTypesOnDispatch = isCheckTypesOnDispatch;
            return this;
        }

        public IBusBuilder MarkerType(Type markerInterface)
        {
            if (!markerInterface.IsInterface)
                throw new ArgumentException("无法指定类标记类型");
            return SetClassChecker(eventType =>
            {
                if (!markerInterface.IsAssignableFrom(eventType))
                    throw new ArgumentException($"{eventType.Name}类型不满足类标记接口要求");
            });
        }

        public IBusBuilder SetClassChecker(EventClassChecker classChecker)
        {
            ClassChecker = classChecker;
            return this;
        }

        public IBusBuilder SetAllowPerPhasePost(bool isAllowPerPhasePost)
        {
            IsAllowPerPhasePost = isAllowPerPhasePost;
            return this;
        }

        public IEventBus Build()
        {
            return new EventBus(this);
        }
    }
}