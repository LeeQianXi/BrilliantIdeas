namespace AvaloniaUtility;

public sealed class Coroutine : IDisposable
{
    private readonly CancellationTokenRegistration _ctr;
    private readonly IEnumerator<YieldInstruction?> _iter;
    private readonly CancellationToken _token;

    public readonly int CoroutineId = -1;
    private bool _disposed;

    private BooleanBox _isActive = false;
    private bool _waiting; // 是否正等待异步指令

    internal Coroutine(IEnumerator<YieldInstruction?> iter, bool createRunning, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(iter, nameof(iter));
        Internal.RegisterInstance(this);
        _iter = iter;
        _token = token;
        if (createRunning)
            Continue();
        else
            Stop();

        _ctr = _token.Register(Dispose);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _iter.Dispose();
        _ctr.Dispose();
    }

    public event Action? Completed;
    public event Action<Exception?>? Faulted;

    private void OnTick(object? sender, EventArgs e)
    {
        if (_disposed || _token.IsCancellationRequested)
        {
            Dispose();
            return;
        }

        try
        {
            if (!_iter.MoveNext())
            {
                CompleteCoroutine();
                return;
            }

            var cur = _iter.Current;
            if (cur is not null) ExecuteYieldInstruction(cur);
        }
        catch (Exception ex)
        {
            HandleCoroutineException(ex);
        }
    }

    private void HandleCoroutineException(Exception exception)
    {
        if (_disposed) return;
        try
        {
            Faulted?.Invoke(exception);
        }
        finally
        {
            Dispose();
        }
    }

    private void ExecuteYieldInstruction(YieldInstruction yi)
    {
        _waiting = true;
        yi.Execute(_token).ContinueWith(OnInstructionCompleted, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void OnInstructionCompleted(Task task)
    {
        if (_disposed) return;
        _waiting = false;
        if (task.IsFaulted)
        {
            var ex = task.Exception?.GetBaseException() ?? new Exception("Unknown coroutine error");
            HandleCoroutineException(ex);
        }
    }

    private void CompleteCoroutine()
    {
        if (_disposed) return;
        try
        {
            Completed?.Invoke();
        }
        finally
        {
            Dispose();
        }
    }

    public void Stop()
    {
        if (!_isActive) return;
        lock (_isActive)
        {
            if (!_isActive) return;
            _isActive = false;
        }
    }

    public void Continue()
    {
        if (_isActive) return;
        lock (_isActive)
        {
            if (_isActive) return;
            _isActive = true;
        }
    }

    public void Close()
    {
        Dispose();
    }

    public override int GetHashCode()
    {
        return CoroutineId;
    }

    private static class Internal
    {
        public const double FrameTime = 1d / 60d;
        private static readonly DispatcherTimer GlobalTimer;
        private static readonly Stopwatch Sw;
        internal static double Accumulator;
        private static readonly Dictionary<int, Coroutine> Instances = new();
        private static readonly FieldInfo CoroutineIdField;

        private static readonly object _sync = new();

        static Internal()
        {
            Sw = Stopwatch.StartNew();
            CoroutineIdField = typeof(Coroutine).GetRuntimeField(nameof(CoroutineId))!;
            GlobalTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(1)
            };
            GlobalTimer.Tick += OnGlobalTick;
            GlobalTimer.Start();
        }

        private static void OnGlobalTick(object? sender, EventArgs e)
        {
            var delta = Sw.Elapsed.TotalSeconds;
            Sw.Restart();
            Accumulator = double.Min(Accumulator + delta, 0.25);
            foreach (var (k, cor) in Instances)
            {
                if (Accumulator < 0) break;
                if (cor._disposed)
                {
                    Instances.Remove(k);
                    continue;
                }

                if (!cor._isActive || cor._waiting) continue;
                cor.OnTick(sender, e);
            }
        }

        public static void RegisterInstance(Coroutine coroutine)
        {
            ArgumentNullException.ThrowIfNull(coroutine, nameof(coroutine));
            lock (_sync)
            {
                var i = Instances.Count;
                while (Instances.ContainsKey(i)) i++;

                CoroutineIdField.SetValue(coroutine, i);
                Instances.Add(i, coroutine);
            }
        }
    }
}