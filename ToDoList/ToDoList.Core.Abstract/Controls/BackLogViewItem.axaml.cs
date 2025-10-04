using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ToDoListDb.Abstract.Model;

namespace ToDoList.Core.Abstract.Controls;

public class BackLogViewItem : TemplatedControl
{
    public BackLogViewItem() : this(BackLog.Empty)
    {

    }

    public BackLogViewItem(BackLog bl)
    {

    }
}