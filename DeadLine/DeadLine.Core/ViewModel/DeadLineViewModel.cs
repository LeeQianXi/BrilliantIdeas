using System.ComponentModel;

namespace DeadLine.Core.ViewModel;

/// <summary>
///     DeadLineWindow的ViewModel,负责数据相关处理和数据库操作
/// </summary>
public partial class DeadLineViewModel : ViewModelBase, IDeadLineViewModel
{
    private readonly SourceList<DeadLineItemInfo> _deadLineItems = new();

    private readonly Subject<DeadLineItemInfo> _deleteDataBaseSubject = new();
    private readonly Subject<Unit> _manual = new();
    private readonly IDeadLineInfoStorage _storage;
    private readonly SemaphoreSlim _storageSemaphore = new(1, 1);
    private readonly Subject<DeadLineItemInfo> _updateOrAddDataBaseSubject = new();

    /// <inheritdoc />
    public DeadLineViewModel(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetRequiredService<ILogger<DeadLineViewModel>>();
        _storage = serviceProvider.GetRequiredService<IDeadLineInfoStorage>();
        const int threshold = 100;
        var circle = new TimeSpan(0, 5, 0);
        var dMerge = _deleteDataBaseSubject
            .Where(i => i.PrimaryKey is not -1)
            .Select(item => (item, true));
        var newMerge = _updateOrAddDataBaseSubject
            .Where(i => i.PrimaryKey is -1)
            .Select(item => (item, false));
        var uMerge = _updateOrAddDataBaseSubject
            .Where(i => i.PrimaryKey is not -1)
            .Select(item => (item, false));

        var merge = dMerge
            .Merge(uMerge)
            .Distinct(p => p.item.PrimaryKey)
            .Merge(newMerge);

        var countBound = _updateOrAddDataBaseSubject
            .Scan(0, (acc, a) => acc + 1)
            .Where(cnt => cnt % threshold is 0)
            .Select(_ => Unit.Default);
        var timeBound = Observable
            .Interval(circle)
            .Select(_ => Unit.Default);
        var bound = _manual
            .Merge(countBound)
            .Merge(timeBound);

        merge.Buffer(bound)
            .Where(b => b.Count > 0)
            .Select(l => Observable.FromAsync(() => OnUpdateToDatabaseAsync(l)))
            .Concat()
            .Subscribe();
    }

    public IObservable<IChangeSet<DeadLineItemInfo>> DeadLineItemsConnect()
    {
        return _deadLineItems.Connect();
    }

    public override IServiceProvider ServiceProvider { get; }
    public override ILogger Logger { get; }
    public Interaction<INewDeadLineItemView, DeadLineItemInfo?> ShowDialogInteraction { get; } = new();
    public Interaction<IEditItemInfoWindow, ConsumeFactory<DeadLineItemInfo>?> EditItemInfoInteraction { get; } = new();

    public async IAsyncEnumerable<DeadLineItemInfo> LoadDatabase()
    {
        if (!await _storageSemaphore.WaitAsync(0))
        {
            Logger.LogInformation("Deadline items are already being loaded");
            yield break;
        }

        try
        {
            await foreach (var items in _storage.SelectDatasAsync(50))
            {
                Logger.LogInformation("Load Next Range of DeadLineItems");
                foreach (var item in items)
                    yield return item;
            }
        }
        finally
        {
            _storageSemaphore.Release();
        }
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

        LogSuccessGetNewDeadlineItemDeadLineItem(Logger, lii);
        DisplayDeadLineItemCommand.Execute(lii);
        _updateOrAddDataBaseSubject.OnNext(lii);
    }

    [RelayCommand]
    private void DisplayDeadLineItem(DeadLineItemInfo lii)
    {
        Logger.LogInformation("Add New DeadLineItem To Display");
        _deadLineItems.Add(lii);
        lii.PropertyChanged += DealItemPropertyChanged;
        lii.RemoveClickEvent += DealRemoveItem;
        lii.EditClickEvent += DealEditItem;
        return;

        void DealItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not DeadLineItemInfo info) return;
            if (!(e.PropertyName switch
                {
                    nameof(DeadLineItemInfo.Status) => true,
                    nameof(DeadLineItemInfo.Title) => true,
                    nameof(DeadLineItemInfo.Description) => true,
                    _ => false
                })) return;
            LogReasonUpdateToDatabase(Logger, $"Property ${e.PropertyName} of Key:${info.PrimaryKey} Changed");
            _updateOrAddDataBaseSubject.OnNext(lii);
        }

        void DealRemoveItem(DeadLineItemInfo removal)
        {
            _deadLineItems.Remove(removal);
            LogReasonDeleteFromDatabase(Logger, $"Key:{removal.PrimaryKey} will be removed");
            _deleteDataBaseSubject.OnNext(removal);
        }

        void DealEditItem(DeadLineItemInfo source)
        {
            Logger.LogInformation("Try Edit DeadLineItem Info");
            var editor = ServiceProvider.GetRequiredService<IEditItemInfoWindow>();
            editor.SourceItem = source;
            EditItemInfoInteraction.Handle(editor).Subscribe(consume =>
            {
                if (consume is null)
                {
                    Logger.LogInformation("No Edit Occured on DeadLineItem Info");
                    return;
                }

                consume.Invoke(source);
                Logger.LogInformation("Success Edit DeadLineItem Info");
                _updateOrAddDataBaseSubject.OnNext(source);
            });
        }
    }

    [RelayCommand]
    private void SaveToDatabase()
    {
        foreach (var deadLineItemInfo in _deadLineItems.Items) _updateOrAddDataBaseSubject.OnNext(deadLineItemInfo);
        _manual.OnNext(Unit.Default);
        Logger.LogInformation("Call Update To Database Now");
    }

    private async Task OnUpdateToDatabaseAsync(IList<(DeadLineItemInfo, bool)> batch)
    {
        LogUpdateDeadlineitemsOfCountCount(Logger, batch.Count);
        await _storage.BeginTransactionAsync(con =>
        {
            foreach (var (item, flag) in batch)
            {
                //true为删除,false为更新
                if (flag)
                    con.Delete<DeadLineItemInfo>(item.PrimaryKey);
                if (item.PrimaryKey is -1)
                    con.Insert(item);
                con.Update(item);
            }
        });
        Logger.LogInformation("Success Update DeadLineItems");
    }
}