namespace DeadLine.Core.ViewModel;

public partial class DeadLineViewModel
{
    [LoggerMessage(LogLevel.Information, "{Reason} ,Add to Update Stream")]
    static partial void LogReasonUpdateToDatabase(ILogger logger, string reason);

    [LoggerMessage(LogLevel.Information, "{Reason} ,Add to Delete Stream")]
    static partial void LogReasonDeleteFromDatabase(ILogger logger, string reason);

    [LoggerMessage(LogLevel.Information, "Success Get new Deadline Item {deadlineItem}")]
    static partial void LogSuccessGetNewDeadlineItemDeadLineItem(ILogger logger, DeadLineItemInfo deadlineItem);

    [LoggerMessage(LogLevel.Information, "Update DeadLineItems of Count {Count}")]
    static partial void LogUpdateDeadlineitemsOfCountCount(ILogger logger, int count);
}