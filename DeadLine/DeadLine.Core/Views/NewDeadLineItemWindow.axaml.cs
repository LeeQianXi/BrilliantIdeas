namespace DeadLine.Core.Views;

public partial class NewDeadLineItemWindow : Window, INewDeadLineItemView
{
    private const string StartLaterThanEnd = "开始日期不能晚于截止日期";

    public NewDeadLineItemWindow()
    {
        InitializeComponent();
        InitForm();
    }

    private void InitForm()
    {
        TbTitle.Clear();
        CdpStart.SelectedDate = DateTime.Today;
        CdpEnd.SelectedDate = DateTime.Today.AddDays(1);
        TpStart.SelectedTime = TpEnd.SelectedTime = DateTime.Now.TimeOfDay;
    }

    private DeadLineItemInfo GetDeadLineItemInfo()
    {
        var (start, end) = GetCurrentInfoTime();
        return new DeadLineItemInfo(TbTitle.Text!, start, end);
    }

    private (DateTime Start, DateTime End) GetCurrentInfoTime()
    {
        var start = CdpStart.SelectedDate!.Value.Date.Add(TpStart.SelectedTime!.Value);
        var end = CdpEnd.SelectedDate!.Value.Date.Add(TpEnd.SelectedTime!.Value);
        return (start, end);
    }

    private void PopupWarning(string message)
    {
    }

    private void ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(TbTitle.Text))
            TbTitle.Text = TbTitle.Watermark;
    }

    private void BtnCreate_OnClick(object? sender, RoutedEventArgs e)
    {
        ValidateForm();
        Close(GetDeadLineItemInfo());
    }

    private void BtnCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ValidateTimeWhenSameDay(bool editEnd)
    {
        if (TpStart.SelectedTime is null) return;
        if (TpEnd.SelectedTime is null) return;
        if (TpStart.SelectedTime < TpEnd.SelectedTime) return;
        PopupWarning(StartLaterThanEnd);
        if (editEnd)
            TpEnd.Clear();
        else
            TpStart.Clear();
    }

    private void CdpStart_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CdpEnd.SelectedDate is null) return;
        if (CdpStart.SelectedDate is null) return;
        if (CdpStart.SelectedDate < CdpEnd.SelectedDate) return;
        if (CdpStart.SelectedDate == CdpEnd.SelectedDate)
        {
            ValidateTimeWhenSameDay(true);
            return;
        }

        PopupWarning(StartLaterThanEnd);
        CdpEnd.Clear();
    }

    private void CdpEnd_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CdpEnd.SelectedDate is null) return;
        if (CdpStart.SelectedDate is null) return;
        if (CdpStart.SelectedDate < CdpEnd.SelectedDate) return;
        if (CdpStart.SelectedDate == CdpEnd.SelectedDate)
        {
            ValidateTimeWhenSameDay(false);
            return;
        }

        PopupWarning(StartLaterThanEnd);
        CdpStart.Clear();
    }

    private void TpStart_OnSelectedTimeChanged(object? sender, TimePickerSelectedValueChangedEventArgs e)
    {
        if (CdpEnd.SelectedDate is null) return;
        if (CdpStart.SelectedDate is null) return;
        if (CdpStart.SelectedDate < CdpEnd.SelectedDate) return;
        ValidateTimeWhenSameDay(true);
    }

    private void TpEnd_OnSelectedTimeChanged(object? sender, TimePickerSelectedValueChangedEventArgs e)
    {
        if (CdpEnd.SelectedDate is null) return;
        if (CdpStart.SelectedDate is null) return;
        if (CdpStart.SelectedDate < CdpEnd.SelectedDate) return;
        ValidateTimeWhenSameDay(false);
    }
}