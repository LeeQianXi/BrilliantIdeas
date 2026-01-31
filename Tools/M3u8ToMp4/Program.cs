using NetUtility.CommandParser;

namespace M3u8ToMp4;

internal static class Program
{
    private static readonly CommandLineParser Parser;

    private static bool _enableRecursive;
    private static bool _ignoreSingleton;
    private static int _recursiveDepth;
    private static bool _enableAsyncDealing;

    static Program()
    {
        Parser = CommandLineParser.Builder()
            .AddSwitchArgument("r") //递归识别
            .AddOptionalArguments("deep", "4") //最大递归深度
            .AddSwitchArgument("a") //启用异步解析
            .AddSwitchArgument("i") //忽略单个任务解析失败
            .AddParamArguments() //启用不定参数个数
            .AddSwitchArgument("h")
            .Build();
    }

    private static CancellationTokenSource CancellationTokenSource { get; } = new();
    private static List<Task> Tasks { get; } = [];

    public static int Main(string[] args)
    {
        if (args.Length is 0)
        {
            Parser.ShowHelp();
            return 0;
        }

        CommandParseResult result = null!;
        try
        {
            result = Parser.Parse(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            Parser.ShowHelp();
            return 1;
        }

        if (result.HasSwitchArg("h"))
        {
            Parser.ShowHelp();
            return 1;
        }

        _enableRecursive = result.HasSwitchArg("r");
        _enableAsyncDealing = result.HasSwitchArg("a");
        _recursiveDepth = int.TryParse(result.GetOptionalArg("deep"), out var rd) ? rd : 4;
        _ignoreSingleton = result.HasSwitchArg("i");
        Console.WriteLine("M3U8 视频合并工具");
        Console.WriteLine("==================");
        foreach (var paramArg in result.GetParamArgs())
            DealPath(paramArg);
        Task.WaitAll(Tasks.ToArray());
        return 0;
    }

    private static void DealPath(string path)
    {
        if (File.Exists(path))
        {
            DealFile(path);
            return;
        }

        if (Directory.Exists(path))
        {
            DealDir(path);
            return;
        }

        Console.WriteLine($"Unknown path: {path}");
    }

    private static void DealDir(string path, int curd = 0)
    {
        Console.WriteLine($"Search Directory: {path}");
        foreach (var file in Directory.GetFiles(path, "*.m3u8")) DealFile(file);

        if (!_enableRecursive) return;
        if (curd > _recursiveDepth) return;
        foreach (var directory in Directory.GetDirectories(path)) DealDir(directory, curd + 1);
    }

    private static void DealFile(string path)
    {
        if (!path.EndsWith(".m3u8")) return;
        try
        {
            var builder = M3U8MergerBase.Builder(path);
            if (_enableAsyncDealing)
                Tasks.Add(builder.Async().Build().Merge(CancellationTokenSource.Token));
            else
                builder.Build().Merge();
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception)
        {
            if (!_ignoreSingleton)
                throw;
        }
    }
}