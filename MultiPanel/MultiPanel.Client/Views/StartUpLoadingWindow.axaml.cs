using Avalonia.Controls;
using AvaloniaUtility;
using AvaloniaUtility.Services;
using Microsoft.Extensions.Logging;

namespace MultiPanel.Client.Views;

public partial class StartUpLoadingWindow : Window, IStartupWindow, ICoroutinator
{
    private readonly ILogger _logger;

    public StartUpLoadingWindow()
    {
        _logger = ServiceLocator.Instance.GetLogger<StartUpLoadingWindow>();
        InitializeComponent();
        this.StartCoroutine(ConnectionVerify);
        ServiceLocator.Instance.ClientContext.TryConnect();
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
    }
}