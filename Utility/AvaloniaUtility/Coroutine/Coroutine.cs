namespace AvaloniaUtility;

public sealed class Coroutine : IDisposable
{
    private readonly IEnumerator<YieldInstruction?> _iter;
    private readonly CancellationToken _token;
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _sw;
    private readonly CancellationTokenRegistration _ctr;
    private double _accumulator;
    private bool _waiting; // 是否正等待异步指令
    private bool _disposed;
    public event Action? Completed;
    public event Action<Exception?>? Faulted;

    internal Coroutine(IEnumerator<YieldInstruction?> iter, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(iter, nameof(iter));
        _iter = iter;
        _token = token;
        _sw = Stopwatch.StartNew();
        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(1)
        };
        _timer.Tick += OnTick;
        _timer.Start();
        _ctr = _token.Register(Dispose);
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_disposed || _token.IsCancellationRequested)
        {
            Dispose();
            return;
        }

        var delta = _sw.Elapsed.TotalSeconds;
        _sw.Restart();
        _accumulator = double.Min(_accumulator + delta, 0.25);

        while (!_waiting && _accumulator >= 0 && !_disposed)
        {
            if (!ExecuteCoroutineStep())
            {
                return;
            }
        }
    }

    private bool ExecuteCoroutineStep()
    {
        try
        {
            if (!_iter.MoveNext())
            {
                CompleteCoroutine();
                return false;
            }

            var cur = _iter.Current;
            if (cur is not null)
            {
                ExecuteYieldInstruction(cur);
                return false; // 暂停执行，等待异步操作完成
            }
            else
            {
                const double frameTime = 1d / 60d;
                _accumulator -= frameTime;
            }

            return true;
        }
        catch (Exception ex)
        {
            HandleCoroutineException(ex);
            return false;
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
        else if (!task.IsCanceled)
        {
            _sw.Restart();
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
        Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Tick -= OnTick;
        _iter.Dispose();
        _ctr.Dispose();
    }
}