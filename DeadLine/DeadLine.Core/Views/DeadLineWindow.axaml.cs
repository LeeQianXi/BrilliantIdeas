namespace DeadLine.Core.Views;

public partial class DeadLineWindow : ViewModelWindowBase<IDeadLineViewModel>, IStartupWindow, IDeadLineView,
    ICoroutinator
{
    private readonly Coroutine _coroutine;

    private bool _isClosed;

    public DeadLineWindow()
    {
        InitializeComponent();
        _coroutine = this.StartCoroutine(TimeSpanSave);
        this.StartCoroutine(LoadExistedDeadLineItems);
        ViewModel!.ShowDialogInteraction.RegisterHandler(ShowDialogInteraction);
    }

    public CancellationTokenSource CancellationTokenSource { get; } = new();


    private async Task ShowDialogInteraction(IInteractionContext<INewDeadLineItemView, DeadLineItemInfo?> arg)
    {
        arg.SetOutput(await arg.Input.ShowDialog<DeadLineItemInfo>(this));
    }

    private IEnumerator<YieldInstruction?> LoadExistedDeadLineItems()
    {
        foreach (var item in ViewModel!.LoadDeadLineItems(CancellationTokenSource.Token))
        {
            ViewModel!.AddDeadLineItemCommand.Execute(item);
            yield return null;
        }
    }

    private IEnumerator<YieldInstruction?> TimeSpanSave()
    {
        while (!_isClosed)
        {
            yield return new WaitForSeconds(TimeSpan.FromMinutes(5));
            ViewModel!.SaveDeadLineItemsCommand.Execute(null);
        }
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _isClosed = true;
        _coroutine.Close();
        ViewModel!.SaveDeadLineItemsCommand.ExecuteAsync(null);
    }
}