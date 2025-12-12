// ReSharper disable CheckNamespace

namespace AvaloniaUtility;

public sealed class Coroutine : IDisposable
{
    private readonly IAsyncEnumerator<YieldInstruction?>? _asyncIter;
    private readonly CancellationTokenRegistration _ctr;
    private readonly bool _isAsync;
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
    private BooleanBox _isActive = false;

    /// <summary>
    ///     是否正等待YieldInstruction指令
    /// </summary>
    private bool _waiting;

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
        if (_disposed) return;
        _disposed = true;
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

    private void OnTick()
    {
        var start = Internal.Sw.Elapsed.TotalMilliseconds;
        if (_disposed || _token.IsCancellationRequested)
        {
            Dispose();
            return;
        }

        try
        {
            if (!MoveToNextInstruction())
            {
                CompleteCoroutine();
                return;
            }

            if (_currentInstruction is null) return;
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
    private bool MoveToNextInstruction()
    {
        try
        {
            return _isAsync ? MoveToNextAsync().GetAwaiter().GetResult() : MoveToNextSync();
        }
        catch (OperationCanceledException)
        {
            throw; // 不包装
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
        // 检查取消令牌
        if (_token.IsCancellationRequested)
            _token.ThrowIfCancellationRequested();
        // 异步移动
        if (!await _asyncIter!.MoveNextAsync().ConfigureAwait(false)) return false;
        _currentInstruction = _asyncIter.Current;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MoveToNextSync()
    {
        // 检查取消令牌
        if (_token.IsCancellationRequested)
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
            .ContinueWith(OnInstructionCompleted, TaskScheduler.FromCurrentSynchronizationContext());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnInstructionCompleted(Task task)
    {
        if (_disposed) return;
        _waiting = false;
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
            GlobalTimer = new DispatcherTimer(DispatcherPriority.Render)
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
            if (Accumulator >= FrameTime)
                OnGlobalFrame();
            Accumulator %= FrameTime;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void OnGlobalFrame()
        {
            lock (ToAdd)
            {
                if (ToAdd.Count is not 0)
                {
                    Instances.UnionWith(ToAdd);
                    ToAdd.Clear();
                }
            }

            List<Coroutine>? toRemove = null;
            foreach (var cor in Instances)
            {
                if (cor._disposed)
                    (toRemove ??= []).Add(cor);
                if (!cor._isActive || cor._waiting) continue;
                Dispatcher.UIThread.Invoke(cor.OnTick);
            }

            if (toRemove is not null)
                Instances.UnionWith(toRemove);
        }

        public static void RegisterInstance(Coroutine coroutine)
        {
            ArgumentNullException.ThrowIfNull(coroutine);
            CoroutineIdField.SetValue(coroutine, _nextId);
            try
            {
                lock (ToAdd)
                {
                    ToAdd.Add(coroutine);
                }

                _nextId++;
            }
            catch (ArgumentException e)
            {
                //TODO:整理序数
            }
        }
    }
}