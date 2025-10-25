namespace AvaloniaUtility;
public interface ICoroutinator
{
    CancellationTokenSource CancellationTokenSource { get; }
}

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public static class Extensions
{
    extension(ICoroutinator cor)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coroutine StartCoroutine(Func<IEnumerator<YieldInstruction?>> routine)
        {
            ArgumentNullException.ThrowIfNull(cor, nameof(cor));
            ArgumentNullException.ThrowIfNull(routine, nameof(routine));
            return new Coroutine(routine.Invoke(), cor.CancellationTokenSource.Token);
        }
    }
}