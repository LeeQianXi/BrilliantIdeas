using System.Diagnostics;

namespace NetUtility;

public sealed class DisposableStopWatch(Action<long> onElapsedMilliseconds) : IDisposable
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public void Dispose()
    {
        _stopwatch.Stop();
        var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
        onElapsedMilliseconds(elapsedMilliseconds);
    }
}