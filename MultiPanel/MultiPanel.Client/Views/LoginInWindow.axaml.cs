using AvaloniaUtility;
using AvaloniaUtility.Services;
using AvaloniaUtility.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiPanel.Client.Abstract.ViewModels;
using MultiPanel.Interfaces.IGrains;
using MultiPanel.Shared.Services;

namespace MultiPanel.Client.Views;

public partial class LoginInWindow : ViewModelWindowBase<ILoginInViewModel>, IStartupWindow, ICoroutinator
{
    public LoginInWindow()
    {
        InitializeComponent();
        this.StartCoroutine(ConnectionVerify);
    }

    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    private async IAsyncEnumerator<YieldInstruction?> ConnectionVerify()
    {
        ViewModel!.Logger.LogInformation("Trying to connect to server");
        yield return null;
        if (await ServiceLocator.Instance.ClientContext.TryConnect())
            await ConnectionSuccess();
        else
            await ConnectionFailed();
    }

    private async Task ConnectionFailed()
    {
        ViewModel!.Logger.LogWarning("Connection failed, Program will be closed soon.");
        await Task.Delay(500);
        Close();
    }

    private async Task ConnectionSuccess()
    {
        ViewModel!.Logger.LogInformation("Connection successful, Jump to Main Window soon.");
        await Task.Delay(500);

        var uag = ServiceLocator.Instance.ClientContext.Client.GetGrain<IUserAccountGrain>("Dev");
        if (await uag.ValidatePassword("123456"))
        {
            ViewModel!.Logger.LogInformation("Successfully validated password");
        }
        else
        {
            ViewModel!.Logger.LogInformation("Failed to validate password");
            await uag.SetPasswordHash(ServiceLocator.Instance.ServiceProvider.GetRequiredService<IPasswordHasher>()
                .Hash("123456"));
            ViewModel!.Logger.LogInformation("Successfully change password");
        }

        if (await uag.ValidatePassword("123456"))
        {
            var ui = await uag.GetUserInfo();
        }
    }
}