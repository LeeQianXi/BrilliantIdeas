namespace DeadLine.Core.Abstract.Converters;

public class TimeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            DateTime dateTime => Format(dateTime),
            TimeSpan timeSpan => Format(new DateTime(timeSpan.Ticks)),
            _ => value
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 如果需要双向绑定，在这里实现反向转换
        throw new NotSupportedException();
    }

    private static string Format(DateTime time)
    {
        if (time.Year > 1)
            return $"{time.Year - 1}年{time.DayOfYear - 1}日";
        if (time.DayOfYear > 1)
            return $"{time.DayOfYear - 1}日{time.Hour}时";
        if (time.Hour > 0)
            return $"{time.Hour}时{time.Minute}分{time.Second}秒";
        if (time.Minute > 0)
            return $"{time.Minute}分{time.Second}秒";
        if (time.Second > 0)
            return $"{time.Second}秒";
        return "任务已截止";
    }
}