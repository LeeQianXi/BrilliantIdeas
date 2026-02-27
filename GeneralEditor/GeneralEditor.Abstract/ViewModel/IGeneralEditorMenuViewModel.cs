using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using DIAbstract;
using ReactiveUI;

namespace GeneralEditor.Core.Abstract.ViewModel;

public interface IGeneralEditorMenuViewModel : IDependencyInjection
{
    Interaction<INotification, Unit> DisplayNotifyInteraction { get; }
    ReadOnlyObservableCollection<string> ControlTitles { get; }
    Control? CurrentControl { get; }
    IAsyncRelayCommand<string> ExecuteControlCommand { get; }
}