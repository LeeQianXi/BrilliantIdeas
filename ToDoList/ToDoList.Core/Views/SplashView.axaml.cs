using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaUtility;
using PropertyGenerator.Avalonia;
using ToDoList.Core.Abstract.ViewModels;

namespace ToDoList.Core.Views;

public partial class SplashView : ViewModelUserControlBase<ISplashViewModel>, ICoroutinator
{
    public SplashView()
    {
        InitializeComponent();
    }

    private Coroutine? _coroutine;

    private void Splash_Loaded(object? sender, RoutedEventArgs e)
    {
        _coroutine = this.StartCoroutine(LoadDataAsync);
        _coroutine.Completed += SplashCompleted;
        _coroutine.Completed += ViewModel!.SplashCompleted;
    }

    private void SplashCompleted()
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(1),
            Easing = new SineEaseOut(),
            FillMode = FillMode.Forward,
            PlaybackDirection = PlaybackDirection.Normal,
            IterationCount = new IterationCount(1),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.3d),
                    Setters = { new Setter(OpacityProperty, 1d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(OpacityProperty, 0d) }
                }
            }
        };
        animation.RunAsync(this).ContinueWith(_ => Dispatcher.UIThread.Post(() => IsVisible = false));
    }

    private IEnumerator<YieldInstruction?> LoadDataAsync()
    {
        PbPro.ShowProgressText = false;
        PbPro.ProgressTextFormat = "{0:0}/{3:0}";
        PbPro.IsIndeterminate = true;
        TbInfo.Text = "Prepare Loading...";
        yield return new WaitForSeconds(TimeSpan.FromSeconds(1));

        TbInfo.Text = "Loading Groups...";
        var iter = ViewModel!.LoadGroupInfo();
        while (!iter.MoveNext())
        {
            yield return iter.Current;
        }

        TbInfo.Text = "Loading Tasks...";
        iter = ViewModel!.LoadTaskInfo();
        while (!iter.MoveNext())
        {
            yield return iter.Current;
        }

        PbPro.ShowProgressText = false;
        PbPro.Classes.Add("Success");
        TbInfo.Text = "Loading Complete";
        yield return new WaitForTask(ViewModel!.ApplyInitWith());
    }

    public CancellationTokenSource CancellationTokenSource { get; } = new();
}