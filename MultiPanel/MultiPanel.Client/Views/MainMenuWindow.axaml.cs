using AvaloniaUtility.Views;
using MultiPanel.Client.Abstract.ViewModels;
using MultiPanel.Client.Abstract.Views;

namespace MultiPanel.Client.Views;

public partial class MainMenuWindow : ViewModelWindowBase<IMainMenuViewModel>, IMainMenuView
{
    public MainMenuWindow()
    {
        InitializeComponent();
    }
}