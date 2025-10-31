using AvaloniaUtility.Views;

namespace DLManager.Core.Views;

public partial class DlManagerView : ViewModelWindowBase<IDlManagerViewModel>, IStartupWindow, IDlManagerView
{
    public DlManagerView()
    {
        InitializeComponent();
    }
}