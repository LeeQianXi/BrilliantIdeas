namespace AvaloniaUtility;

public interface IDebouncer
{
}

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public static partial class Extensions
{
    extension(IDebouncer deb)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DebounceBase CreateDebouncer(Func<CancellationToken, Task> debounceAction)
        {
            return deb.CreateDebouncer(debounceAction, TimeSpan.FromMilliseconds(500));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DebounceBase CreateDebouncer(Action debounceAction)
        {
            return deb.CreateDebouncer(debounceAction, TimeSpan.FromMilliseconds(500));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DebounceBase CreateDebouncer(Func<CancellationToken, Task> debounceAction, TimeSpan delay)
        {
            return new AsyncDebounce(debounceAction, delay);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DebounceBase CreateDebouncer(Action debounceAction, TimeSpan delay)
        {
            return new Debounce(debounceAction, delay);
        }
    }
}