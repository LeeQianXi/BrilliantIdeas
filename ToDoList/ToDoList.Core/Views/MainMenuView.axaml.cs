namespace ToDoList.Core.Views;

public partial class MainMenuView : ViewModelUserControlBase<IMainMenuViewModel>, IMainMenuView
{
    private readonly ObservableCollection<BackLogViewItem> _displayItems = [];

    public MainMenuView()
    {
        InitializeComponent();
    }
}