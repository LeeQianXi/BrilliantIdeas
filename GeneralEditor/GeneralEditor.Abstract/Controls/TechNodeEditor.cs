using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using GeneralEditor.Core.Abstract.Services;

namespace GeneralEditor.Core.Abstract.Controls;

[TemplatePart("PART_CoreCanvas", typeof(Canvas))]
public sealed partial class TechNodeEditor : TemplatedControl
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _coreCanvas = e.NameScope.Get<Canvas>("PART_CoreCanvas");
        BindEvents();
    }

    partial void BindEvents();

    private Canvas? _coreCanvas;

    protected override void OnInitialized()
    {
    }

    partial void BindEvents()
    {
        _coreCanvas!.RenderTransform = _scaleTransform;
#if DEBUG
        var btn = new Button()
        {
            Width = 100,
            Height = 60,
            Content = "TestScale",
        };
        btn.SetValue(Canvas.LeftProperty, 150);
        _coreCanvas.Children.Add(btn);
#endif
    }
    private readonly ScaleTransform _scaleTransform=new();

    protected override void OnKeyDown(KeyEventArgs e)
    {
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
    }

    public override void Render(DrawingContext context)
    {
    }
}

[ControlProvider]
public sealed class TechNodeEditorProvider : IControlProvider
{
    public string Title => "TechNodeEditor";

    public Control GetControl(IServiceProvider serviceProvider)
    {
        return new TechNodeEditor();
    }
}

[ControlProvider]
public sealed class TechNodeEditorProvider2 : IControlProvider
{
    public string Title => "TechNodeEditor2";

    public Control GetControl(IServiceProvider serviceProvider)
    {
        return new TechNodeEditor();
    }
}