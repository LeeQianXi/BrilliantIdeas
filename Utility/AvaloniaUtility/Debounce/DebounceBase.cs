namespace AvaloniaUtility;

public abstract class DebounceBase : IDisposable
{
    private readonly object _lockObject = new();
    private readonly DispatcherTimer _timer;

    private bool _disposed;
    private CancellationTokenSource? _tokenSource;

    protected DebounceBase(TimeSpan interval)
    {
        Interval = interval;
        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = interval
        };
        _timer.Tick += OnTimerTick;
    }

    protected TimeSpan Interval { get; }

    public bool IsPending
    {
        get
        {
            lock (_lockObject)
            {
                return _timer.IsEnabled;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Call()
    {
        ThrowIfDisposed();
        lock (_lockObject)
        {
            _timer.Stop();
            _timer.Start();
        }
    }

    public async Task FlushAsync()
    {
        ThrowIfDisposed();

        lock (_lockObject)
        {
            if (!_timer.IsEnabled)
                return;

            _timer.Stop();
        }

        await ExecuteDebounceAsync();
    }

    public void Cancel()
    {
        ThrowIfDisposed();

        lock (_lockObject)
        {
            _timer.Stop();
            CancelCurrentOperation();
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        lock (_lockObject)
        {
            _timer.Stop();
        }

        // 使用 discard 避免警告，不等待执行完成
        _ = ExecuteDebounceAsync();
    }

    private async Task ExecuteDebounceAsync()
    {
        CancelCurrentOperation();

        _tokenSource = new CancellationTokenSource();
        var token = _tokenSource.Token;

        try
        {
            await ApplyDebounce(token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // 操作被取消，这是预期的行为
        }
        catch (Exception ex)
        {
            // 记录异常，但不重新抛出以避免应用崩溃
            OnDebounceError(ex);
        }
    }

    private void CancelCurrentOperation()
    {
        if (_tokenSource == null) return;
        try
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // 忽略已释放的token source
        }

        _tokenSource = null;
    }

    /// <summary>
    ///     子类实现的防抖逻辑
    /// </summary>
    /// <param name="token">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    protected abstract Task ApplyDebounce(CancellationToken token);

    /// <summary>
    ///     防抖操作发生错误时的回调（子类可重写）
    /// </summary>
    /// <param name="exception">发生的异常</param>
    protected virtual void OnDebounceError(Exception exception)
    {
        // 默认实现记录到调试输出
        Debug.WriteLine($"Debounce error: {exception.Message}");
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                lock (_lockObject)
                {
                    _timer.Stop();
                    _timer.Tick -= OnTimerTick;
                    CancelCurrentOperation();
                }

            _disposed = true;
        }
    }

    ~DebounceBase()
    {
        Dispose(false);
    }
}