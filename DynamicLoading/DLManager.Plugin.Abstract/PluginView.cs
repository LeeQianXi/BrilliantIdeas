namespace DLManager.Plugin.Abstract;

//无需显示调用InitializeComponent
public abstract class PluginView : UserControl
{
    protected PluginView()
    {
        //GetType().GetRuntimeMethod("InitializeComponent", [typeof(bool), typeof(bool)])!.Invoke(this, [true, true]);
    }
}