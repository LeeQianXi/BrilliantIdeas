namespace NetUtility.SimpleLogger;

/// <summary>
///     游戏框架日志类。
/// </summary>
public static partial class LightLog
{
    public static ILogHelper? LogHelper { get; set; }
    public static LogLevel Level { get; set; } = LogLevel.Info;

    public static void Debug(object message)
    {
        if (LogLevel.Debug < Level) return;
        LogHelper?.Log(LogLevel.Debug, message);
    }

    public static void Debug(string message)
    {
        if (LogLevel.Debug < Level) return;
        LogHelper?.Log(LogLevel.Debug, message);
    }

    public static void Debug<T>(string format, T arg)
    {
        if (LogLevel.Debug < Level) return;
        LogHelper?.Log(LogLevel.Debug, Utility.Text.Format(format, arg));
    }

    public static void Debug<T1, T2>(string format, T1 arg1, T2 arg2)
    {
        if (LogLevel.Debug < Level) return;
        LogHelper?.Log(LogLevel.Debug, Utility.Text.Format(format, arg1, arg2));
    }

    public static void Debug<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
    {
        if (LogLevel.Debug < Level) return;
        LogHelper?.Log(LogLevel.Debug, Utility.Text.Format(format, arg1, arg2, arg3));
    }

    public static void Info(object message)
    {
        if (LogLevel.Info < Level) return;
        LogHelper?.Log(LogLevel.Info, message);
    }

    public static void Info(string message)
    {
        if (LogLevel.Info < Level) return;
        LogHelper?.Log(LogLevel.Info, message);
    }

    public static void Info<T>(string format, T arg)
    {
        if (LogLevel.Info < Level) return;
        LogHelper?.Log(LogLevel.Info, Utility.Text.Format(format, arg));
    }

    public static void Info<T1, T2>(string format, T1 arg1, T2 arg2)
    {
        if (LogLevel.Info < Level) return;
        LogHelper?.Log(LogLevel.Info, Utility.Text.Format(format, arg1, arg2));
    }

    public static void Info<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
    {
        if (LogLevel.Info < Level) return;
        LogHelper?.Log(LogLevel.Info, Utility.Text.Format(format, arg1, arg2, arg3));
    }

    public static void Warning(object message)
    {
        if (LogLevel.Warning < Level) return;
        LogHelper?.Log(LogLevel.Warning, message);
    }

    public static void Warning(string message)
    {
        if (LogLevel.Warning < Level) return;
        LogHelper?.Log(LogLevel.Warning, message);
    }

    public static void Warning<T>(string format, T arg)
    {
        if (LogLevel.Warning < Level) return;
        LogHelper?.Log(LogLevel.Warning, Utility.Text.Format(format, arg));
    }

    public static void Warning<T1, T2>(string format, T1 arg1, T2 arg2)
    {
        if (LogLevel.Warning < Level) return;
        LogHelper?.Log(LogLevel.Warning, Utility.Text.Format(format, arg1, arg2));
    }

    public static void Warning<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
    {
        if (LogLevel.Warning < Level) return;
        LogHelper?.Log(LogLevel.Warning, Utility.Text.Format(format, arg1, arg2, arg3));
    }

    public static void Error(object message)
    {
        if (LogLevel.Error < Level) return;
        LogHelper?.Log(LogLevel.Error, message);
    }

    public static void Error(string message)
    {
        if (LogLevel.Error < Level) return;
        LogHelper?.Log(LogLevel.Error, message);
    }

    public static void Error<T>(string format, T arg)
    {
        if (LogLevel.Error < Level) return;
        LogHelper?.Log(LogLevel.Error, Utility.Text.Format(format, arg));
    }

    public static void Error<T1, T2>(string format, T1 arg1, T2 arg2)
    {
        if (LogLevel.Error < Level) return;
        LogHelper?.Log(LogLevel.Error, Utility.Text.Format(format, arg1, arg2));
    }

    public static void Error<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
    {
        if (LogLevel.Error < Level) return;
        LogHelper?.Log(LogLevel.Error, Utility.Text.Format(format, arg1, arg2, arg3));
    }

    public static void Fatal(object message)
    {
        if (LogLevel.Fatal < Level) return;
        LogHelper?.Log(LogLevel.Fatal, message);
    }

    public static void Fatal(string message)
    {
        if (LogLevel.Fatal < Level) return;
        LogHelper?.Log(LogLevel.Fatal, message);
    }

    public static void Fatal<T>(string format, T arg)
    {
        if (LogLevel.Fatal < Level) return;
        LogHelper?.Log(LogLevel.Fatal, Utility.Text.Format(format, arg));
    }

    public static void Fatal<T1, T2>(string format, T1 arg1, T2 arg2)
    {
        if (LogLevel.Fatal < Level) return;
        LogHelper?.Log(LogLevel.Fatal, Utility.Text.Format(format, arg1, arg2));
    }

    public static void Fatal<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
    {
        if (LogLevel.Fatal < Level) return;
        LogHelper?.Log(LogLevel.Fatal, Utility.Text.Format(format, arg1, arg2, arg3));
    }
}