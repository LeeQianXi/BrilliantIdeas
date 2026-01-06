namespace DeadLine.Core.Abstract.View;

public interface IEditItemInfoWindow : IWindow
{
    DeadLineItemInfo SourceItem { get; set; }
}