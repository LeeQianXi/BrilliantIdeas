// ReSharper disable CheckNamespace

namespace AvaloniaUtility;

public interface ICoroutinator
{
    CancellationTokenSource CoroutinatorCancelTokenSource { get; }
}

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public static partial class Extensions
{
    extension(ICoroutinator cor)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coroutine StartCoroutine(Func<IEnumerator<YieldInstruction?>> routine, bool createRunning = true)
        {
            ArgumentNullException.ThrowIfNull(cor);
            ArgumentNullException.ThrowIfNull(routine);
            return new Coroutine(routine.Invoke(), createRunning, cor.CoroutinatorCancelTokenSource.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coroutine StartCoroutine(Func<IAsyncEnumerator<YieldInstruction?>> routine, bool createRunning = true)
        {
            ArgumentNullException.ThrowIfNull(cor);
            ArgumentNullException.ThrowIfNull(routine);
            return new Coroutine(routine.Invoke(), createRunning, cor.CoroutinatorCancelTokenSource.Token);
        }
    }
}