// ReSharper disable CheckNamespace

namespace AvaloniaUtility;

public sealed class Coroutine : IDisposable
{
    private readonly IAsyncEnumerator<YieldInstruction?>? _asyncIter;
    private readonly CancellationTokenRegistration _ctr;
    private readonly bool _isAsync;
    private readonly object _stateLock = new();
    private readonly IEnumerator<YieldInstruction?>? _syncIter;
    private readonly CancellationToken _token;

    public readonly int CoroutineId = -1;

    /// <summary>
    ///     当前YieldInstruction
    /// </summary>
    private YieldInstruction? _currentInstruction;

    private bool _disposed;

    /// <summary>
    ///     当前Corotine是否激活
    /// </summary>
    private bool _isActive;

    /// <summary>
    ///     是否正等待YieldInstruction指令
    /// </summary>
    private volatile bool _waiting;

    internal Coroutine(IEnumerator<YieldInstruction?> iter, bool createRunning, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(iter);
        _isAsync = false;
        _syncIter = iter;
        _token = token;
        if (createRunning)
            Continue();
        else
            Stop();
        _ctr = _token.Register(Dispose);
        Internal.RegisterInstance(this);
    }

    internal Coroutine(IAsyncEnumerator<YieldInstruction?> iter, bool createRunning, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(iter);
        _isAsync = true;
        _asyncIter = iter;
        _token = token;
        if (createRunning)
            Continue();
        else
            Stop();

        _ctr = _token.Register(Dispose);
        Internal.RegisterInstance(this);
    }

    public void Dispose()
    {
        var shouldDispose = false;
        lock (_stateLock) //保护_disposed检查
        {
            if (!_disposed)
            {
                shouldDispose = true;
                _disposed = true;
                _isActive = false;
            }
        }

        if (!shouldDispose) return;

        _ctr.Dispose();
        _syncIter?.Dispose();
        if (_asyncIter != null)
            _ = _asyncIter.DisposeAsync().AsTask();
    }

    /// <summary>
    ///     Coroutine完成回调
    /// </summary>
    public event Action? Completed;

    /// <summary>
    ///     Coroutine失败回调
    /// </summary>
    public event Action<Exception?>? Faulted;

    private async Task OnTick()
    {
        // 在锁内检查状态，避免竞态
        lock (_stateLock)
        {
            if (_disposed || _token.IsCancellationRequested || _waiting || !_isActive)
                return;
        }

        var start = Internal.Sw.Elapsed.TotalMilliseconds;
        try
        {
            // 每个await后重新检查状态
            lock (_stateLock)
            {
                if (_disposed) return;
            }

            var hasNext = await MoveToNextInstruction().ConfigureAwait(false);

            if (!hasNext)
            {
                CompleteCoroutine();
                return;
            }

            if (_currentInstruction is not null)
                ExecuteYieldInstruction();
        }
        catch (OperationCanceledException)
        {
            Dispose();
        }
        catch (Exception ex)
        {
            HandleCoroutineException(ex);
        }
        finally
        {
            Internal.Accumulator -= Internal.Sw.Elapsed.TotalMilliseconds - start;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async Task<bool> MoveToNextInstruction()
    {
        try
        {
            return _isAsync ? await MoveToNextAsync() : MoveToNextSync();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // 包装并重新抛出迭代器异常
            throw new Exception(
                "Failed to move to next instruction in coroutine.",
                ex
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async Task<bool> MoveToNextAsync()
    {
        _token.ThrowIfCancellationRequested();
        var tmp = _waiting;
        _waiting = true;
        if (!await _asyncIter!.MoveNextAsync().ConfigureAwait(false)) return false;
        _waiting = tmp;
        // 检查 disposed
        lock (_stateLock)
        {
            if (_disposed) return false;
        }

        _currentInstruction = _asyncIter.Current;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MoveToNextSync()
    {
        // 检查取消令牌
        _token.ThrowIfCancellationRequested();
        // 同步移动
        if (!_syncIter!.MoveNext()) return false;
        _currentInstruction = _syncIter.Current;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteYieldInstruction()
    {
        if (_currentInstruction is null) return;
        _waiting = true;
        _currentInstruction.Execute(_token)
            .ContinueWith(OnInstructionCompleted, TaskScheduler.Current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnInstructionCompleted(Task task)
    {
        _waiting = false;
        if (_disposed) return;
        if (!task.IsFaulted) return;
        var ex = task.Exception?.GetBaseException() ?? new Exception("Unknown coroutine error");
        HandleCoroutineException(ex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        lock (_stateLock)
        {
            if (!_isActive) return;
            _isActive = false;
        }
    }

    public void Continue()
    {
        lock (_stateLock)
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
        public const double FrameTime = 1_000d / 90d;
        public const double MaxFrameTime = 1_000d / 10d;

        private static readonly DispatcherTimer GlobalTimer;
        internal static readonly Stopwatch Sw;
        internal static double Accumulator;

        private static int _nextId;
        private static readonly HashSet<Coroutine> Instances = [];
        private static readonly List<Coroutine> ToAdd = [];
        private static readonly FieldInfo CoroutineIdField;

        static Internal()
        {
            CoroutineIdField = typeof(Coroutine).GetRuntimeField(nameof(CoroutineId))!;

            Sw = Stopwatch.StartNew();
            GlobalTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1)
            };
            GlobalTimer.Tick += OnGlobalTick;
            GlobalTimer.Start();
        }

        private static void OnGlobalTick(object? sender, EventArgs e)
        {
            var delta = Sw.Elapsed.TotalMilliseconds;
            Sw.Restart();
            Accumulator += delta;
            if (Accumulator > FrameTime)
                OnGlobalFrame();
            Accumulator %= FrameTime;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void OnGlobalFrame()
        {
            lock (ToAdd)
            {
                if (ToAdd.Count != 0)
                {
                    Instances.UnionWith(ToAdd);
                    ToAdd.Clear();
                }
            }

            List<Coroutine>? toRemove = null;
            foreach (var cor in Instances)
            {
                bool isDisposed, isActive, isWaiting;
                lock (cor._stateLock) // 读取状态时加锁
                {
                    isDisposed = cor._disposed;
                    isActive = cor._isActive;
                    isWaiting = cor._waiting;
                }

                if (isDisposed)
                {
                    (toRemove ??= []).Add(cor);
                    continue;
                }

                if (!isActive || isWaiting) continue;
                Dispatcher.UIThread.Invoke(cor.OnTick);
            }

            if (toRemove is null) return;
            Instances.ExceptWith(toRemove);
        }

        public static void RegisterInstance(Coroutine coroutine)
        {
            ArgumentNullException.ThrowIfNull(coroutine);

            lock (ToAdd) // 在锁内分配ID
            {
                CoroutineIdField.SetValue(coroutine, Interlocked.Increment(ref _nextId));
                ToAdd.Add(coroutine);
            }
        }
    }
}