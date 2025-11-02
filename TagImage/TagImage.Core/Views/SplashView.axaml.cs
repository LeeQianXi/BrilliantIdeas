namespace TagImage.Core.Views;

public partial class SplashView : ViewModelUserControlBase<ISplashViewModel>, ISplashView, ICoroutinator
{
    private Coroutine? _coroutine;

    public SplashView()
    {
        InitializeComponent();
    }

    public CancellationTokenSource CancellationTokenSource { get; } = new();

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
        var step = (PbPro.Maximum - PbPro.Minimum) / 100d;
        while (PbPro.Value < PbPro.Maximum)
        {
            PbPro.Value += step;
            yield return null;
        }
    }
}