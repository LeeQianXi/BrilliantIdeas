namespace BreaTest;

public class CoroutineTest : ICoroutinator
{
    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    [Fact]
    public void TestSync()
    {
        var count = 1000;
        foreach (var i in Enumerable.Range(0, count))
            this.StartCoroutine(SyncAction).Completed += CompleteCount;
        while (count <= 0) ;
        return;

        void CompleteCount()
        {
            count--;
        }
    }

    [Fact]
    public void TestAsync()
    {
        var count = 1000;
        foreach (var i in Enumerable.Range(0, count))
            this.StartCoroutine(AsyncAction).Completed += CompleteCount;
        while (count <= 0) ;
        return;

        void CompleteCount()
        {
            count--;
        }
    }

    private static IEnumerator<YieldInstruction?> SyncAction()
    {
        var l = new List<int>();
        foreach (var i in Enumerable.Range(0, 1000))
        {
            l.Add(i);
            yield return null;
            Debug.WriteLine("SyncAction: " + i);
        }

        Display(l);
    }

    private static async IAsyncEnumerator<YieldInstruction?> AsyncAction()
    {
        var l = new List<int>();
        foreach (var i in Enumerable.Range(0, 1000))
        {
            l.Add(i);
            yield return null;
            Debug.WriteLine("AsyncAction: " + i);
        }

        Display(l);
    }

    private static void Display(IEnumerable<int> l)
    {
        Console.WriteLine(string.Join(", ", l));
    }
}