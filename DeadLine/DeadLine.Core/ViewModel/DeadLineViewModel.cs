using System.ComponentModel;
using Avalonia.Controls.Notifications;
using AvaloniaUtility.Controls;
using LiveChartsCore.SkiaSharpView;
using NetUtility.RefPool;

namespace DeadLine.Core.ViewModel;

/// <summary>
///     DeadLineWindow的ViewModel,负责数据相关处理和数据库操作
/// </summary>
public partial class DeadLineViewModel : ViewModelBase, IDeadLineViewModel
{
    private readonly SourceList<DeadLineItemInfo> _deadLineItems = new();

    private readonly Subject<DeadLineItemInfo> _deleteDataBaseSubject = new();
    private readonly Subject<Unit> _itemRefreshSubject = new();
    private readonly Subject<Unit> _manual = new();

    private readonly ReadOnlyObservableCollection<PieSeries<int>> _pieSeries;
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

        #region Statistical

        var connect = _deadLineItems.Connect();
        _deadLineItems.CountChanged.ObserveOn(RxApp.MainThreadScheduler)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.TotalCount, out _totalCount);
        connect.LiveStat(q => q.Count(x => x.Status is DeadLineStatus.ToDo))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.ToDoCount, out _toDoCount);
        connect.LiveStat(q => q.Count(x => x.Status is DeadLineStatus.Doing))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.DoingCount, out _doingCount);
        connect.LiveStat(q => q.Count(x => x.Status is DeadLineStatus.Done))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.DoneCount, out _doneCount);
        connect.LiveStat(q => q.Count(x => x.Status is DeadLineStatus.TimedOut))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.TimedOutCount, out _timedOutCount);
        connect.LiveStat(q => q.Count(x => x.Status is DeadLineStatus.ToDo or DeadLineStatus.Doing))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.NotCompletedCount, out _notCompletedCount);
        connect.LiveStat(q =>
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                return q.Count(x =>
                {
                    var end = x.EndTime;
                    return end >= today && end < tomorrow;
                });
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.DueTodayCount, out _dueTodayCount);
        connect.LiveStat(q =>
            {
                var mon = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var sun = mon.AddDays(6).AddDays(1).AddTicks(-1);
                return q.Count(x =>
                {
                    var end = x.EndTime;
                    return end >= mon && end <= sun;
                });
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.DueWeekCount, out _dueWeekCount);

        #endregion
    }

    public IEnumerable<PieSeries<int>> Series => _pieSeries;

    public IObservable<IChangeSet<DeadLineItemInfo>> DeadLineItemsConnect()
    {
        return _deadLineItems.Connect();
    }


    public override IServiceProvider ServiceProvider { get; }
    public override ILogger Logger { get; }
    public Interaction<INewDeadLineItemView, DeadLineItemInfo?> ShowDialogInteraction { get; } = new();
    public Interaction<IEditItemInfoWindow, ConsumeFactory<DeadLineItemInfo>?> EditItemInfoInteraction { get; } = new();
    public Interaction<INotification, Unit> NotifyScreenInteraction { get; } = new();


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

            Logger.LogInformation("Successfully Loaded All Ranges of DeadLineItems");
        }
        finally
        {
            _storageSemaphore.Release();
        }
    }

    public void RefreshData()
    {
        _itemRefreshSubject.OnNext(Unit.Default);
    }


    private async Task PostNotification(string title, string message,
        NotificationType type = NotificationType.Information)
    {
        var notification = ReferencePool.Acquire<ReferenceNotification>();
        notification.Init(title, message, type);
        await NotifyScreenInteraction.Handle(notification);
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
        await PostNotification("系统消息", "成功创建新任务", NotificationType.Success);
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
            _deadLineItems.Edit(li => li.ReplaceOrAdd(info, info));
            _updateOrAddDataBaseSubject.OnNext(lii);
        }

        void DealRemoveItem(DeadLineItemInfo removal)
        {
            _deadLineItems.Remove(removal);
            LogReasonDeleteFromDatabase(Logger, $"Key:{removal.PrimaryKey} will be removed");
            PostNotification("系统消息", "成功删除任务", NotificationType.Success);
            _deleteDataBaseSubject.OnNext(removal);
        }

        void DealEditItem(DeadLineItemInfo source)
        {
            Logger.LogInformation("Try Edit DeadLineItem Info");
            var editor = ServiceProvider.GetRequiredService<IEditItemInfoWindow>();
            editor.InitSource(source);
            EditItemInfoInteraction.Handle(editor)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(consume =>
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

    #region Statistical

    private readonly ObservableAsPropertyHelper<int> _totalCount;
    public int TotalCount => _totalCount.Value;

    private readonly ObservableAsPropertyHelper<int> _doneCount;
    public int DoneCount => _doneCount.Value;

    private readonly ObservableAsPropertyHelper<int> _doingCount;
    public int DoingCount => _doingCount.Value;

    private readonly ObservableAsPropertyHelper<int> _toDoCount;
    public int ToDoCount => _toDoCount.Value;

    private readonly ObservableAsPropertyHelper<int> _timedOutCount;
    public int TimedOutCount => _timedOutCount.Value;

    private readonly ObservableAsPropertyHelper<int> _dueTodayCount;
    public int DueTodayCount => _dueTodayCount.Value;

    private readonly ObservableAsPropertyHelper<int> _dueWeekCount;
    public int DueWeekCount => _dueWeekCount.Value;

    private readonly ObservableAsPropertyHelper<int> _notCompletedCount;
    public int NotCompletedCount => _notCompletedCount.Value;

    #endregion
}