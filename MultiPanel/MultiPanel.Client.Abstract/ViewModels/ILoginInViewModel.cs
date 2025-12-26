using System.Reactive;
using CommunityToolkit.Mvvm.Input;
using DIAbstract.Services;
using ReactiveUI;

namespace MultiPanel.Client.Abstract.ViewModels;

public interface ILoginInViewModel : IDependencyInjection
{
    bool RememberMe { get; set; }
    string Username { get; set; }
    string Password { get; set; }
    public IAsyncRelayCommand LoginCommand { get; }
    public Interaction<string, Unit> WarningInfo { get; }
}