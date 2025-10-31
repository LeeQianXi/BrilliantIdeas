namespace M3u8ToMp4;

internal static class Program
{
    private static readonly CommandLineParser Parser;

    private static bool _enableRecursive;
    private static bool _ignoreSingleton;
    private static int _recursiveDepth;
    private static bool _enableAsyncDealing;
    private static readonly List<Task> AsyncDealingTasks = [];

    static Program()
    {
        Parser = CommandLineParser.Builder()
            .AddSwitchArgument("r") //递归识别
            .AddOptionalArguments("deep", "4") //最大递归深度
            .AddSwitchArgument("a") //启用异步解析
            .AddSwitchArgument("i") //忽略单个任务解析失败
            .AddParamArguments() //启用不定参数个数
            .Build();
    }

    public static void Main(string[] args)
    {
        if (args.Length is 0) args = [Environment.CurrentDirectory];
        var param = Parser.Parse(args);
        _enableRecursive = param.HasSwitchArg("r");
        _enableAsyncDealing = param.HasSwitchArg("a");
        _recursiveDepth = int.TryParse(param.GetOptionalArg("deep"), out var rd) ? rd : 4;
        _ignoreSingleton = param.HasSwitchArg("i");
        Console.WriteLine("M3U8 视频合并工具");
        Console.WriteLine("==================");
        foreach (var paramArg in param.GetParamArgs())
            DealPath(paramArg);
        Task.WaitAll(AsyncDealingTasks);
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
            if (_enableAsyncDealing)
                M3U8Merger.Builder(path)
                    .Build()
                    .Run();
            else
                AsyncDealingTasks.Add(
                    M3U8Merger.Builder(path)
                        .Async()
                        .Build()
                        .Run());
        }
        catch (Exception)
        {
            if (!_ignoreSingleton)
                throw;
        }
    }
}