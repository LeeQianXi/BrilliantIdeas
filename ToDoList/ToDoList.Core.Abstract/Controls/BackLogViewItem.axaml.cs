using TaskStatus = ToDoList.DataBase.Models.TaskStatus;

namespace ToDoList.Core.Abstract.Controls;

[TemplatePart(Name = "PART_Title", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Tag", Type = typeof(Label))]
[TemplatePart(Name = "PART_Select", Type = typeof(ListBox))]
public partial class BackLogViewItem : TemplatedControl
{
    public static readonly DirectProperty<BackLogViewItem, int> GroupIdProperty =
        AvaloniaProperty.RegisterDirect<BackLogViewItem, int>(nameof(GroupId),
            static o => o.GroupId);

    [GeneratedStyledProperty("Default")] public partial string Title { get; set; }

    [GeneratedStyledProperty("")] public partial string Description { get; set; }

    [GeneratedStyledProperty(TaskStatus.Default)]
    public partial TaskStatus Status { get; set; }

    public int GroupId
    {
        get;
        set => SetAndRaise(GroupIdProperty, ref field, value);
    }

    partial void OnStatusPropertyChanged(TaskStatus newValue)
    {
        if (_tag is null) return;
        var (t, c) = newValue switch
        {
            TaskStatus.InProgress => ("Running", "LightBlue"),
            TaskStatus.Completed => ("Done", "Green"),
            TaskStatus.Failed => ("Failed", "Red"),
            TaskStatus.Ignored => ("Skipped", "Grey"),
            _ => ("Pending", "Orange")
        };
        _tag!.Content = t;
        _tag.Classes.Clear();
        _tag.Classes.Add("Ghost");
        _tag.Classes.Add(c);
        _select!.SelectedIndex = (int)newValue;
    }

    #region Controls

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _tag = e.NameScope.Get<Label>("PART_Tag");
        _title = e.NameScope.Get<TextBox>("PART_Title");
        _title.LostFocus += OnTitleLostFocus;
        _select = e.NameScope.Get<ListBox>("PART_Select");
        _select.SelectionChanged += OnSelectionChanged;
        OnStatusPropertyChanged(Status);
    }

    private Label? _tag;
    private TextBox? _title;
    private ListBox? _select;

    private void OnEditTitleClicked(object? sender, RoutedEventArgs e)
    {
        _title!.IsReadOnly = false;
        _title.Focus();
    }

    private void OnTitleLostFocus(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_title!.Text)) _title!.Text = "默认认任务";

        _title!.IsReadOnly = true;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var lb = sender as ListBox;
        Status = (TaskStatus)lb!.SelectedIndex;
    }

    #endregion
}