namespace DeadLine.Core.Views;

public partial class EditItemInfoWindow : Window, IEditItemInfoWindow
{
    public EditItemInfoWindow()
    {
        InitializeComponent();
    }

    public DeadLineItemInfo SourceItem { get; set; }
}