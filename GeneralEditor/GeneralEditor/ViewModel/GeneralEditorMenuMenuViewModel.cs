using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using AvaloniaUtility.Controls;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Alias;
using GeneralEditor.Core.Abstract.Bases;
using GeneralEditor.Core.Abstract.Services;
using GeneralEditor.Core.Abstract.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetUtility.Cache;
using NetUtility.RefPool;
using ReactiveUI;

namespace GeneralEditor.ViewModel;

public partial class GeneralEditorMenuMenuViewModel : ViewModelBase, IGeneralEditorMenuViewModel
{
    private readonly LruCache<string, Control> _controlCache = new(5);
    private readonly ReadOnlyObservableCollection<string> _controlTitles;
    private readonly SourceCache<IControlProvider, string> _providers = new(static cp => cp.Title);

    public GeneralEditorMenuMenuViewModel(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetRequiredService<ILogger<GeneralEditorMenuMenuViewModel>>();
        _providers.AddOrUpdate(serviceProvider.GetServices<IControlProvider>());
        _providers.Connect()
            .Select(cp => cp.Title)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _controlTitles)
            .Subscribe();
        _controlCache.Expired += OnControlExpired;
    }

    public Interaction<INotification, Unit> DisplayNotifyInteraction { get; } = new();

    public override IServiceProvider ServiceProvider { get; }
    public override ILogger Logger { get; }
    public ReadOnlyObservableCollection<string> ControlTitles => _controlTitles;

    public Control? CurrentControl
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    private void OnControlExpired(KeyValuePair<string, Control> pair)
    {
        Logger.LogInformation("Control With Key {ControlKey} Expired", pair.Key);
    }

    [RelayCommand]
    private async Task ExecuteControl(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Logger.LogWarning("Control Key is null or empty");
            await DisplayNotifyInteraction.Handle(GenerateNotification("程序异常", "尝试使用空键", NotificationType.Warning));
            return;
        }

        var opt = _providers.Lookup(key);
        if (!opt.HasValue)
        {
            Logger.LogWarning("Control Key {KetValue} is not registered", key);
            await DisplayNotifyInteraction.Handle(GenerateNotification("程序异常", "尝试使用未注册的键", NotificationType.Error));
        }
        else
        {
            Logger.LogInformation("Successful Load Control and Cache Control");
            await DisplayNotifyInteraction.Handle(GenerateNotification("通知", "成功加载编辑器", NotificationType.Success));
            CurrentControl = _controlCache.GetOrAdd(key, _ => opt.Value.GetControl(ServiceProvider));
        }
    }

    private static INotification GenerateNotification(string title, string message,
        NotificationType type = NotificationType.Information)
    {
        var notification = ReferenceNotification.AcquireReference();
        notification.Init(title, message, type);
        return notification;
    }
}