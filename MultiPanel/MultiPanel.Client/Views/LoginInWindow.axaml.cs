using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;
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
    private Animation? _displayWarningAnimation;
    private CancellationTokenSource? _displayWarningAnimationTokenSource;

    public LoginInWindow()
    {
        InitializeComponent();
        ViewModel!.DisplayToScreen.RegisterHandler(DisplayWarningInfo);
        ViewModel.SuccessLoginInteraction.RegisterHandler(SuccessLoginWindow);
        this.StartCoroutine(ConnectionVerify);
    }

    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    private void DisplayWarningInfo(IInteractionContext<string, Unit> context)
    {
        _displayWarningAnimationTokenSource?.Cancel();
        _displayWarningAnimationTokenSource = new CancellationTokenSource();
        PART_WainingText.Text = context.Input;
        _displayWarningAnimation ??= InitAnimation();
        _displayWarningAnimation.RunAsync(PART_WainingContainer, _displayWarningAnimationTokenSource.Token);
        context.SetOutput(Unit.Default);
        return;

        Animation InitAnimation()
        {
            return new Animation
            {
                Easing = new QuarticEaseInOut(),
                Duration = TimeSpan.FromSeconds(3.5),
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0),
                        Setters =
                        {
                            new Setter(OpacityProperty, 1.0)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(0.6),
                        Setters =
                        {
                            new Setter(OpacityProperty, 1.0)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters =
                        {
                            new Setter(OpacityProperty, 0.0)
                        }
                    }
                }
            };
        }
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
            Logger.LogError("Failed to connect to server");
            await ViewModel.DisplayToScreen.Handle("无法连接到服务器,请稍候再试");
            yield break;
        }

        ViewModel.Logger.LogInformation("Successfully connected to server");
        yield return null;
        if (!ViewModel.RememberMe)
            yield break;
        ViewModel.Logger.LogInformation("Trying to login to server with old data");
        var dto = await ViewModel.LoginWithRememberMeAsync();
        if (dto is null)
        {
            ViewModel.Logger.LogWarning("Failed to login to server with old data");
            yield break;
        }

        ViewModel.Logger.LogInformation("Successfully logged in to server");
        await ViewModel.SuccessLoginCommand.ExecuteAsync(dto);
        yield break;
    }
}