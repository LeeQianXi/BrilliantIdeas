namespace NetUtility.EventSystem;

internal sealed partial class EventBus
{
    private sealed class ListenerList
    {
        private static readonly EventPriority[] Priorities = Enum.GetValues<EventPriority>();
        private readonly bool _buildPerPhaseList;
        private readonly bool _canUnwrapListeners;
        private readonly ListenerList? _parent;

        private readonly List<List<IEventListener>> _priorities;
        private readonly SemaphoreSlim _writeLock = new(1, 1); // fair, 1 permit
        private List<ListenerList>? _children;
        private volatile IEventListener[]? _listeners;
        private volatile IEventListener[][]? _perPhaseListeners;

        private volatile bool _rebuild = true;

        public ListenerList(Type eventType, bool buildPerPhaseList) : this(eventType, null, buildPerPhaseList)
        {
        }

        public ListenerList(Type eventType, ListenerList? parent, bool buildPerPhaseList)
        {
            var count = Priorities.Length;
            _priorities = new List<List<IEventListener>>(count);
            for (var i = 0; i < count; i++)
                _priorities.Add([]);

            // Unwrap if the event is not cancellable
            _canUnwrapListeners = !typeof(ICancellableEvent).IsAssignableFrom(eventType);
            _buildPerPhaseList = buildPerPhaseList;

            _parent = parent;
            parent?.AddChild(this);
        }

        /// <summary>
        ///     Returns all listeners for the given priority level, including parent's.
        ///     Children listeners come first.
        /// </summary>
        private List<IEventListener> GetListeners(EventPriority priority)
        {
            _writeLock.Wait();
            try
            {
                var ret = new List<IEventListener>(_priorities[(int)priority]);
                if (_parent is not null)
                    ret.AddRange(_parent.GetListeners(priority));
                return ret;
            }
            finally
            {
                _writeLock.Release();
            }
        }

        /// <summary>
        ///     All listeners across all priorities, in proper order, including parents.
        /// </summary>
        public IEventListener[] GetListeners()
        {
            if (ShouldRebuild()) BuildCache();
            return _listeners!;
        }

        /// <summary>
        ///     Listeners for a single priority level (per-phase cache).
        /// </summary>
        public IEventListener[] GetPhaseListeners(EventPriority phase)
        {
            if (!_buildPerPhaseList)
                throw new InvalidOperationException("buildPerPhaseList is false!");

            if (ShouldRebuild()) BuildCache();
            return _perPhaseListeners![(int)phase];
        }

        private bool ShouldRebuild()
        {
            return _rebuild;
        }

        public void ForceRebuild()
        {
            _rebuild = true;
            if (_children == null) return;

            lock (_children)
            {
                foreach (var child in _children)
                    child.ForceRebuild();
            }
        }

        private void AddChild(ListenerList child)
        {
            _children ??= [];
            lock (_children)
            {
                _children.Add(child);
            }
        }

        private void BuildCache()
        {
            if (_parent is not null && _parent.ShouldRebuild())
                _parent.BuildCache();

            var all = new List<IEventListener>();
            var perPhase = _buildPerPhaseList
                ? new IEventListener[Priorities.Length][]
                : null;

            foreach (var phase in Priorities)
            {
                var phaseListeners = GetListeners(phase);
                UnwrapListeners(phaseListeners);
                all.AddRange(phaseListeners);
                perPhase?[(int)phase] = phaseListeners.ToArray();
            }

            _listeners = all.ToArray();
            _perPhaseListeners = perPhase;
            _rebuild = false;
        }

        private void UnwrapListeners(List<IEventListener> list)
        {
            if (!_canUnwrapListeners) return;

            for (var i = 0; i < list.Count; i++)
                if (list[i] is IWrapperListener wrapper)
                    list[i] = wrapper.GetWithoutCheck();
        }

        public void Register(EventPriority priority, IEventListener listener)
        {
            _writeLock.Wait();
            try
            {
                _priorities[(int)priority].Add(listener);
            }
            finally
            {
                _writeLock.Release();
            }

            ForceRebuild();
        }

        public void Unregister(IEventListener listener)
        {
            _writeLock.Wait();
            try
            {
                foreach (var _ in _priorities.Where(l => l.Remove(listener)))
                    ForceRebuild();
            }
            finally
            {
                _writeLock.Release();
            }
        }
    }
}