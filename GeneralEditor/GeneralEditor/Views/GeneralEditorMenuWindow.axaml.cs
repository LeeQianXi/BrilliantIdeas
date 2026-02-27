using System.Reactive;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using AvaloniaUtility.Services;
using AvaloniaUtility.Views;
using GeneralEditor.Core.Abstract.ViewModel;
using ReactiveUI;

namespace GeneralEditor.Views;

public partial class GeneralEditorMenuWindow : ViewModelWindowBase<IGeneralEditorMenuViewModel>, IStartupWindow
{
    public GeneralEditorMenuWindow()
    {
        InitializeComponent();
        ViewModel!.DisplayNotifyInteraction.RegisterHandler(OnInteractionDisplayWarning);
#if DEBUG
        ControlSelector.SelectedIndex = 0;
#endif
    }

    private void OnInteractionDisplayWarning(IInteractionContext<INotification, Unit> context)
    {
        NotificationManager.Show(context.Input);
        context.SetOutput(Unit.Default);
    }

    private void CurrentControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count is 0 ||
            e.AddedItems.Cast<object>().First() is not string key ||
            string.IsNullOrWhiteSpace(key)) return;
        ViewModel!.ExecuteControlCommand.Execute(key);
    }
}