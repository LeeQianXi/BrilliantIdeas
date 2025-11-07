namespace DeadLine.Core.Views;

public partial class DeadLineWindow : ViewModelWindowBase<IDeadLineViewModel>, IStartupWindow, IDeadLineView
{
    public DeadLineWindow()
    {
        InitializeComponent();
    }
}