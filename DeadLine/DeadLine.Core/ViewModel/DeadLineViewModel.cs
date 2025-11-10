namespace DeadLine.Core.ViewModel;

public partial class DeadLineViewModel(IServiceProvider serviceProvider) : ViewModelBase, IDeadLineViewModel, IDebouncer
{
    private readonly SourceList<DeadLineItemInfo> _deadLineItems = new();
    private readonly IDeadLineInfoStorage _storage = serviceProvider.GetRequiredService<IDeadLineInfoStorage>();
    private bool _isLoaded;
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<DeadLineViewModel>>();
    public Interaction<INewDeadLineItemView, DeadLineItemInfo?> ShowDialogInteraction { get; } = new();

    public IEnumerable<DeadLineItemInfo> LoadDeadLineItems(CancellationToken token)
    {
        if (_isLoaded)
        {
            Logger.LogInformation("Deadline items have already been loaded");
            yield break;
        }

        IEnumerable<DeadLineItemInfo> items = [];
        var completed = false;
        _storage.BeginTransactionAsync(con =>
        {
            items = items.Concat(con.Table<DeadLineItemInfo>().ToList());
            Logger.LogInformation("Loading deadline items From DataBase Success");
            completed = true;
        });
        while (!completed) Thread.Sleep(2);
        Logger.LogInformation("Loading deadline items To UIForm");
        foreach (var item in items)
            yield return item;
        _isLoaded = true;
    }

    public IObservable<IChangeSet<DeadLineItemInfo>> DeadLineItemsConnect()
    {
        return _deadLineItems.Connect();
    }

    [RelayCommand]
    private async Task NewDeadLineItem()
    {
        Logger.LogInformation("Try Add a New DeadLineItem");
        var niv = ServiceProvider.GetRequiredService<INewDeadLineItemView>();
        var lii = await ShowDialogInteraction.Handle(niv);
        if (lii is null)
        {
            Logger.LogWarning("Failed to get New DeadLineItem");
            return;
        }

        Logger.LogInformation("Success Get new deadline item {lii}", lii);
        await AddDeadLineItemCommand.ExecuteAsync(lii);
    }

    [RelayCommand]
    private async Task AddDeadLineItem(DeadLineItemInfo lii)
    {
        Logger.LogInformation("Add New DeadLineItem {lii} To Display", lii);
        _deadLineItems.Add(lii);
        lii.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != nameof(DeadLineItemInfo.Status)) return;
            Logger.LogInformation("Status Changed {lii} ,Try to Save to DataBase", lii);
            _storage.UpdateDataAsync((DeadLineItemInfo)sender!);
        };
        lii.RemoveClickEvent += removal =>
        {
            Logger.LogInformation("Remove DeadLineItem {lii} From Display", lii);
            RemoveDeadLineItemCommand.Execute(removal);
        };
        if (!_isLoaded)
        {
            Logger.LogInformation("DeadlineItem {lii} is from DataBase", lii);
            return;
        }

        Logger.LogInformation("Save DeadLineItem {lii} To DataBase", lii);
        if (lii.PrimaryKey is -1 || await _storage.FindDataAsync(lii.PrimaryKey) is null)
            await _storage.InsertDataAsync(lii);
        else
            await _storage.UpdateDataAsync(lii);
    }

    [RelayCommand]
    private async Task RemoveDeadLineItem(DeadLineItemInfo lii)
    {
        Logger.LogInformation("Try  Remove DeadLineItem {key} From Display", lii.PrimaryKey);
        var info = await _storage.FindDataAsync(lii.PrimaryKey);
        if (info is null)
        {
            Logger.LogInformation("DeadLineItem {key} not found in DataBase", lii.PrimaryKey);
            return;
        }

        _deadLineItems.Remove(lii);
        await _storage.DeleteDataAsync(lii.PrimaryKey);
        Logger.LogInformation("Success Remove DeadLineItem {lii} From Display", lii);
        SaveDeadLineItemsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task SaveDeadLineItems()
    {
        await _storage.UpdateDataAsync(_deadLineItems.Items);
        Logger.LogInformation("Save All DeadLineItems");
    }
}