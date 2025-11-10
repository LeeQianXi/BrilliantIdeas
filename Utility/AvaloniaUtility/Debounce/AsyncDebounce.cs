namespace AvaloniaUtility;

public sealed class AsyncDebounce(Func<CancellationToken, Task> action, TimeSpan interval) : DebounceBase(interval)
{
    protected override async Task ApplyDebounce(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        await action(token).ConfigureAwait(false);
    }
}