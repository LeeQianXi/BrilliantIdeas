using Avalonia.Controls.Primitives;

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

    /// <summary>
    ///     获取表单内容
    /// </summary>
    /// <returns></returns>
    private DeadLineItemInfo GetDeadLineItemInfo()
    {
        var (start, end) = GetCurrentInfoTime();
        return new DeadLineItemInfo(TbTitle.Text!, start, end) { Description = TbDesc.Text ?? string.Empty };
    }

    /// <summary>
    ///     获取表单时间内容
    /// </summary>
    /// <returns></returns>
    private (DateTime Start, DateTime End) GetCurrentInfoTime()
    {
        var start = CdpStart.SelectedDate!.Value.Date.Add(TpStart.SelectedTime!.Value);
        var end = CdpEnd.SelectedDate!.Value.Date.Add(TpEnd.SelectedTime!.Value);
        return (start, end);
    }

    private void PopupWarning(string message)
    {
        PART_FlyoutText.Text = message;
        FlyoutBase.ShowAttachedFlyout(PART_Title);
    }

    /// <summary>
    ///     验证格式化内容
    /// </summary>
    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(TbTitle.Text))
            TbTitle.Text = TbTitle.Watermark;
        if (CdpStart.SelectedDate is null ||
            CdpEnd.SelectedDate is null ||
            TpStart.SelectedTime is null ||
            TpEnd.SelectedTime is null) return false;
        var (s, e) = GetCurrentInfoTime();
        return s < e;
    }

    /// <summary>
    ///     创建新任务
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnCreate_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!ValidateForm())
        {
            PopupWarning("表单未填写完成");
            return;
        }

        Close(GetDeadLineItemInfo());
    }

    /// <summary>
    ///     取消创建
    /// </summary>
    private void BtnCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private bool ValidateDate()
    {
        return CdpEnd.SelectedDate is null ||
               CdpStart.SelectedDate is null ||
               CdpStart.SelectedDate < CdpEnd.SelectedDate;
    }

    private bool ValidateTime()
    {
        return TpStart.SelectedTime is null ||
               TpEnd.SelectedTime is null ||
               TpStart.SelectedTime < TpEnd.SelectedTime;
    }

    /// <summary>
    ///     日期相同时处理时间内容
    /// </summary>
    /// <param name="editEnd">修改哪个内容</param>
    private void ValidateTimeWhenSameDay(bool editEnd)
    {
        if (ValidateTime()) return;
        PopupWarning(StartLaterThanEnd);
        if (editEnd)
            TpEnd.Clear();
        else
            TpStart.Clear();
    }

    /// <summary>
    ///     日期开始修改
    /// </summary>
    private void CdpStart_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ValidateDate()) return;
        if (CdpStart.SelectedDate == CdpEnd.SelectedDate)
        {
            ValidateTimeWhenSameDay(true);
            return;
        }

        PopupWarning(StartLaterThanEnd);
        CdpEnd.Clear();
    }

    /// <summary>
    ///     日期结束修改
    /// </summary>
    private void CdpEnd_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ValidateDate()) return;
        if (CdpStart.SelectedDate == CdpEnd.SelectedDate)
        {
            ValidateTimeWhenSameDay(true);
            return;
        }

        PopupWarning(StartLaterThanEnd);
        CdpStart.Clear();
    }

    /// <summary>
    ///     时间开始修改
    /// </summary>
    private void TpStart_OnSelectedTimeChanged(object? sender, TimePickerSelectedValueChangedEventArgs e)
    {
        if (ValidateDate() || TpStart.SelectedTime is null) return;
        ValidateTimeWhenSameDay(true);
    }

    private void TpEnd_OnSelectedTimeChanged(object? sender, TimePickerSelectedValueChangedEventArgs e)
    {
        if (ValidateDate() || TpEnd.SelectedTime is null) return;
        ValidateTimeWhenSameDay(false);
    }
}