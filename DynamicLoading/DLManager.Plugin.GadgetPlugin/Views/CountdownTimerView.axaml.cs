using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using AvaloniaUtility;
using PropertyGenerator.Avalonia;

namespace DLManager.Plugin.GadgetPlugin.Views;

[DeclareView(nameof(CountdownTimerView))]
public partial class CountdownTimerView : PluginView, ICoroutinator
{
    private Coroutine? _coroutine;

    public CountdownTimerView()
    {
        InitializeComponent();
        OnTimerStatePropertyChanged(CountdownTimerState.Default);
        Progress.Minimum = 0;
    }

    [GeneratedStyledProperty] public partial CountdownTimerState TimerState { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; } = new();

    partial void OnTimerStatePropertyChanged(CountdownTimerState newValue)
    {
        const string def = "开始", sto = "继续", run = "暂停";
        switch (newValue)
        {
            case CountdownTimerState.Default:
                BtnSwitch.Content = def;
                BtnArise.IsVisible = false;
                Timer.IsVisible = false;
                Selector.IsVisible = true;
                break;
            case CountdownTimerState.Stopped:
                BtnSwitch.Content = sto;
                BtnArise.IsVisible = true;
                Timer.IsVisible = true;
                Selector.IsVisible = false;
                break;
            case CountdownTimerState.Running:
                BtnSwitch.Content = run;
                BtnArise.IsVisible = true;
                Timer.IsVisible = true;
                Selector.IsVisible = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
        }
    }

    private void TimePicker_OnSelectedTimeChanged(object? sender, TimePickerSelectedValueChangedEventArgs e)
    {
    }

    private void BtnSwitch_OnClick(object? sender, RoutedEventArgs e)
    {
        switch (TimerState)
        {
            case CountdownTimerState.Default:
                _coroutine?.Dispose();
                _coroutine = null;
                if (Picker.SelectedTime is null) return;
                Progress.Maximum = Picker.SelectedTime.Value.TotalSeconds;
                _coroutine = this.StartCoroutine(ProgressTick);
                TimerState = CountdownTimerState.Running;
                break;
            case CountdownTimerState.Running:
                _coroutine!.Stop();
                TimerState = CountdownTimerState.Stopped;
                break;
            case CountdownTimerState.Stopped:
                _coroutine!.Continue();
                TimerState = CountdownTimerState.Running;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(TimerState), TimerState, null);
        }
    }

    private IEnumerator<YieldInstruction?> ProgressTick()
    {
        Progress.Value = Progress.Maximum;
        Progress.Classes.Clear();
        Progress.Classes.Add("Primary");
        while (Progress.Percentage > 0)
        {
            Progress.Value--;
            yield return new WaitForSeconds(1000);
            if (Progress.Percentage < 0.67) break;
        }

        Progress.Classes.Clear();
        Progress.Classes.Add("Secondary");
        while (Progress.Percentage > 0)
        {
            Progress.Value--;
            yield return new WaitForSeconds(1000);
            if (Progress.Percentage < 0.33) break;
        }

        Progress.Classes.Clear();
        Progress.Classes.Add("Error");
        while (Progress.Percentage > 0)
        {
            Progress.Value--;
            yield return new WaitForSeconds(1000);
        }

        CountdownCompleted();
    }


    private void BtnArise_OnClick(object? sender, RoutedEventArgs e)
    {
        _coroutine?.Dispose();
        _coroutine = null;
        CountdownCompleted();
    }

    private void CountdownCompleted()
    {
        TimerState = CountdownTimerState.Default;
        Picker.SelectedTime = TimeSpan.Zero;
    }

    private void Progress_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        Progress.ProgressTextFormat = Progress.Value > 60 * 60
            ? $"{(int)(Progress.Value / 60 / 60):00}:{(int)(Progress.Value / 60) % 60:00}:{Progress.Value % 60:00}"
            : $"{(int)(Progress.Value / 60):00}:{Progress.Value % 60:00}";
    }
}

public enum CountdownTimerState
{
    Default,
    Stopped,
    Running
}