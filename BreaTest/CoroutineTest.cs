using AvaloniaUtility;

namespace BreaTest;

public class CoroutineTest : ICoroutinator
{
    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    [Fact]
    public void Test1()
    {
        this.StartCoroutine(SyncAction);
        this.StartCoroutine(AsyncAction);
    }

    private static IEnumerator<YieldInstruction> SyncAction()
    {
        yield break;
    }

    private static async IAsyncEnumerator<YieldInstruction> AsyncAction()
    {
        yield break;
    }
}