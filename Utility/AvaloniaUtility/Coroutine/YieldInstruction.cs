namespace AvaloniaUtility;

public abstract record YieldInstruction
{
    internal virtual Task Execute(CancellationToken token) => Task.CompletedTask;
}

public sealed record WaitForSeconds : YieldInstruction
{
    private readonly TimeSpan _timeSpan;
    private readonly TimeProvider _timeProvider;

    public WaitForSeconds(int milliseconds) : this(TimeSpan.FromMilliseconds(milliseconds))
    {
    }

    public WaitForSeconds(TimeSpan timeSpan) : this(timeSpan, TimeProvider.System)
    {
    }

    public WaitForSeconds(TimeSpan timeSpan, TimeProvider timeProvider)
    {
        _timeSpan = timeSpan;
        _timeProvider = timeProvider;
    }

    internal override async Task Execute(CancellationToken token)
    {
        await Task.Delay(_timeSpan, _timeProvider, token);
    }
}

public sealed record WaitForTask : YieldInstruction
{
    private readonly Task _task;

    public WaitForTask(Task task)
    {
        _task = task;
    }

    internal override async Task Execute(CancellationToken token)
    {
        await _task.WaitAsync(token);
    }
}