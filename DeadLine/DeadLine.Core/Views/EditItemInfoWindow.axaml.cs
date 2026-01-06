using Avalonia.Controls.Notifications;
using AvaloniaUtility.Controls;
using NetUtility.RefPool;

namespace DeadLine.Core.Views;

public partial class EditItemInfoWindow : Window, IEditItemInfoWindow
{
    private DeadLineItemInfo? _sourceItem;

    public EditItemInfoWindow()
    {
        InitializeComponent();
    }

    public void InitSource(DeadLineItemInfo sourceItem)
    {
        _sourceItem = sourceItem;
        TbTitle.Text = _sourceItem.Title;
        TbDescription.Text = _sourceItem.Description;
    }

    private void BtnCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnConfirm_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TbTitle.Text))
        {
            var notification = ReferencePool.Acquire<ReferenceNotification>();
            notification.Init("表单填写有误", "标题不能为空", NotificationType.Warning);
            Manager.Show(notification);
            return;
        }

        if (_sourceItem is null)
        {
            Close();
            return;
        }

        ConsumeFactory<DeadLineItemInfo>? consume = null;
        if (TbTitle.Text != _sourceItem.Title)
            consume += item => item.Title = TbTitle.Text;
        if (TbDescription.Text != _sourceItem.Description)
            consume += item => item.Description = TbDescription.Text ?? string.Empty;
        Close(consume);
    }
}