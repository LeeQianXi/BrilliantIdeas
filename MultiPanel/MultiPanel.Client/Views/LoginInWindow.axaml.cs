using System.Reactive;
using AvaloniaUtility;
using AvaloniaUtility.Services;
using AvaloniaUtility.Views;
using Microsoft.Extensions.Logging;
using MultiPanel.Abstractions.DTOs;
using MultiPanel.Client.Abstract.ViewModels;
using MultiPanel.Interfaces.IGrains;
using ReactiveUI;

namespace MultiPanel.Client.Views;

public partial class LoginInWindow : ViewModelWindowBase<ILoginInViewModel>, IStartupWindow, ICoroutinator
{
    public LoginInWindow()
    {
        InitializeComponent();
        ViewModel!.WarningInfo.RegisterHandler(DisplayWarningInfo);
        this.StartCoroutine(ConnectionVerify);
    }

    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    private void DisplayWarningInfo(IInteractionContext<string, Unit> context)
    {
        ViewModel!.Logger.LogWarning(context.Input);
    }

    private async IAsyncEnumerator<YieldInstruction?> ConnectionVerify()
    {
        ViewModel!.Logger.LogInformation("Trying to connect to server");
        var context = ServiceLocator.Instance.ClientContext;
        await context.TryConnect();
        yield return null;
        if (!context.IsConnected)
        {
            ViewModel.WarningInfo.Handle("Failed to connect to server");
            yield return new WaitForSeconds(3000);
            Close();
            yield break;
        }

        ViewModel.Logger.LogInformation("Successfully connected to server");
        yield return null;
        if (!ViewModel.RememberMe)
            yield break;
        ViewModel.Logger.LogInformation("Trying to login to server with old token");
        var client = context.Client;
        var data = await ViewModel.TryLoginWithLastedData();
        if (data == AccountInfo.Empty)
            yield break;
        client.GetGrain<ISessionGrain>(data.UserId);
    }
}