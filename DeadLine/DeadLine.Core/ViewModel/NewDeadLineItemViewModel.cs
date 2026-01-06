using Avalonia.Controls.Notifications;
using AvaloniaUtility.Controls;
using FluentValidation;
using NetUtility.RefPool;

namespace DeadLine.Core.ViewModel;

public partial class NewDeadLineItemViewModel(IServiceProvider serviceProvider)
    : ViewModelBase, INewDeadLineItemViewModel
{
    private readonly IValidator<NewDeadLineItemViewModel> _validator =
        serviceProvider.GetRequiredService<IValidator<NewDeadLineItemViewModel>>();

    private DateTime Start => StartDate.Add(StartTime);
    private DateTime End => EndDate.Add(EndTime);

    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<NewDeadLineItemViewModel>>();

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; } = DateTime.Now.Date;

    public DateTime EndDate { get; set; } = DateTime.Now.Date.AddDays(1);

    public TimeSpan StartTime { get; set; } = DateTime.Now.TimeOfDay;

    public TimeSpan EndTime { get; set; } = DateTime.Now.TimeOfDay;

    public Interaction<DeadLineItemInfo?, Unit> CloseIteration { get; } = new();
    public Interaction<INotification, Unit> DisplayWarningInteraction { get; } = new();

    private static INotification GenerateNotification(string title, string message)
    {
        var notification = ReferencePool.Acquire<ReferenceNotification>();
        notification.Init(title, message, NotificationType.Warning);
        return notification;
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await CloseIteration.Handle(null);
    }

    [RelayCommand]
    private async Task CreateItem()
    {
        var result = await _validator.ValidateAsync(this);
        if (!result.IsValid)
        {
            var error = result.Errors.First();
            var notification = GenerateNotification("表单填写有误", error.ErrorMessage);
            await DisplayWarningInteraction.Handle(notification);
            return;
        }

        var newItem = new DeadLineItemInfo
        {
            Title = string.IsNullOrWhiteSpace(Title) ? "新建任务" : Title,
            Description = Description,
            StartTime = Start,
            EndTime = End,
            Status = DateTime.Now < Start ? DeadLineStatus.ToDo :
                DateTime.Now < End ? DeadLineStatus.Doing : DeadLineStatus.TimedOut
        };
        await CloseIteration.Handle(newItem);
    }
}

internal class NewDeadLineItemValidator : AbstractValidator<NewDeadLineItemViewModel>
{
    public NewDeadLineItemValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("开始日期必须早于结束日期");
        RuleFor(x => x.StartDate + x.StartTime)
            .LessThan(x => x.EndDate + x.EndTime).WithMessage("开始时间必须早于结束时间");
        RuleFor(x => x.EndDate + x.EndTime)
            .GreaterThan(DateTime.Now).WithMessage("无法创建一个必定失败的计划");
    }
}