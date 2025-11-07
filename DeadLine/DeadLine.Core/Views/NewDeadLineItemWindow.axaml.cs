namespace DeadLine.Core.Views;

public partial class NewDeadLineItemWindow : Window, INewDeadLineItemView
{
    public NewDeadLineItemWindow()
    {
        InitializeComponent();
    }

    private void BtnTest_OnClick(object? sender, RoutedEventArgs e)
    {
        Close("Success");
    }
}