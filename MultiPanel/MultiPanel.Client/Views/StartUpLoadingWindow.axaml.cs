using Avalonia.Controls;
using AvaloniaUtility;
using AvaloniaUtility.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiPanel.Interfaces.IGrains;
using MultiPanel.Shared.Services;

namespace MultiPanel.Client.Views;

public partial class StartUpLoadingWindow : Window, IStartupWindow, ICoroutinator
{
    private readonly ILogger _logger;

    public StartUpLoadingWindow()
    {
        _logger = ServiceLocator.Instance.GetLogger<StartUpLoadingWindow>();
        InitializeComponent();
        this.StartCoroutine(ConnectionVerify);
    }

    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    private async IAsyncEnumerator<YieldInstruction?> ConnectionVerify()
    {
        _logger.LogInformation("Trying to connect to server");
        yield return null;
        if (await ServiceLocator.Instance.ClientContext.TryConnect())
            await ConnectionSuccess();
        else
            await ConnectionFailed();
    }

    private async Task ConnectionFailed()
    {
        _logger.LogWarning("Connection failed, Program will be closed soon.");
        await Task.Delay(500);
        Close();
    }

    private async Task ConnectionSuccess()
    {
        _logger.LogInformation("Connection successful, Jump to Main Window soon.");
        await Task.Delay(500);

        var uag = ServiceLocator.Instance.ClientContext.Client.GetGrain<IUserAccountGrain>("Dev");
        if (await uag.ValidatePassword("123456"))
        {
            _logger.LogInformation("Successfully validated password");
        }
        else
        {
            _logger.LogInformation("Failed to validate password");
            await uag.SetPasswordHash(ServiceLocator.Instance.ServiceProvider.GetRequiredService<IPasswordHasher>()
                .Hash("123456"));
            _logger.LogInformation("Successfully change password");
        }

        if (await uag.ValidatePassword("123456"))
        {
            var ui = await uag.GetUserInfo();
        }
    }
}