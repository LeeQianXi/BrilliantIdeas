namespace AvaloniaUtility;

public sealed class Debounce(Action action, TimeSpan interval) : DebounceBase(interval)
{
    protected override Task ApplyDebounce(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        action();
        return Task.CompletedTask; // 为了满足返回Task的要求，但不需要异步
    }
}