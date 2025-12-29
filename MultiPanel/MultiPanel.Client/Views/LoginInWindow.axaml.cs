using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using AvaloniaUtility;
using AvaloniaUtility.Services;
using AvaloniaUtility.Views;
using Microsoft.Extensions.Logging;
using MultiPanel.Client.Abstract.ViewModels;
using MultiPanel.Client.Abstract.Views;
using ReactiveUI;

namespace MultiPanel.Client.Views;

public partial class LoginInWindow : ViewModelWindowBase<ILoginInViewModel>, IStartupWindow, ICoroutinator
{
    public LoginInWindow()
    {
        InitializeComponent();
        ViewModel!.WarningInfo.RegisterHandler(DisplayWarningInfo);
        ViewModel.SuccessLoginInteraction.RegisterHandler(SuccessLoginWindow);
        this.StartCoroutine(ConnectionVerify);
    }

    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    private void DisplayWarningInfo(IInteractionContext<string, Unit> context)
    {
        ViewModel!.Logger.LogWarning("{WarningInfo}", context.Input);
        context.SetOutput(Unit.Default);
    }

    private void SuccessLoginWindow(IInteractionContext<IMainMenuView, Unit> context)
    {
        context.Input.Show();
        Close();
        context.SetOutput(Unit.Default);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        ViewModel!.SaveConfigCommand.Execute(null);
        base.OnClosing(e);
    }

    private async IAsyncEnumerator<YieldInstruction?> ConnectionVerify()
    {
        ViewModel!.Logger.LogInformation("Trying to connect to server");
        var context = ServiceLocator.Instance.ClientContext;
        await context.TryConnect();
        yield return null;
        if (!context.IsConnected)
        {
            await ViewModel.WarningInfo.Handle("Failed to connect to server");
            yield return new WaitForSeconds(3000);
            Close();
            yield break;
        }

        ViewModel.Logger.LogInformation("Successfully connected to server");
        yield return null;
        if (!ViewModel.RememberMe)
            yield break;
        ViewModel.Logger.LogInformation("Trying to login to server with old data");
        var dto = await ViewModel.LoginWithRememberMeAsync();
        if (dto is not { IsValid: true })
        {
            ViewModel.Logger.LogWarning("Failed to login to server with old data");
            yield break;
        }

        ViewModel.Logger.LogInformation("Successfully logged in to server");
        await ViewModel.SuccessLoginCommand.ExecuteAsync(dto);
        yield break;
    }
}