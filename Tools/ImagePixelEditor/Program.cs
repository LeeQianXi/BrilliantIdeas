namespace ImagePixelEditor;

internal static class Program
{
    private static readonly CommandLineParser Parser;

    private static bool _enableRecursive;
    private static bool _ignoreSingleton;
    private static int _recursiveDepth;
    private static bool _enableAsyncDealing;
    private static bool _saveNewFile;
    private static readonly List<Task> AsyncDealingTasks = [];

    private static Rgba32 _sourceColor;
    private static Rgba32 _targetColor;
    private static double _colorBias;

    private static readonly ConcurrentDictionary<Rgba32, Rgba32> ColorReplaceCache = new();

    private static readonly IImageEncoder _encoder = new PngEncoder
    {
        ColorType = PngColorType.RgbWithAlpha,
        BitDepth = PngBitDepth.Bit8,
        TransparentColorMode = PngTransparentColorMode.Preserve
    };

    static Program()
    {
        Parser = CommandLineParser.Builder()
            .AddSwitchArgument("r") //递归识别
            .AddOptionalArguments("deep", "4") //最大递归深度
            .AddSwitchArgument("a") //启用异步解析
            .AddSwitchArgument("i") //忽略单个任务解析失败
            .AddRequiredArguments("source") //源颜色
            .AddRequiredArguments("target") //目标颜色
            .AddOptionalArguments("bias", "5") //颜色偏差值
            .AddSwitchArgument("nf")
            .AddParamArguments() //启用不定参数个数
            .Build();
    }

    [STAThread]
    private static void Main(string[] args)
    {
        var result = Parser.Parse(args);
        _enableRecursive = result.HasSwitchArg("r");
        _recursiveDepth = int.Parse(result.GetOptionalArg("deep"));
        _enableAsyncDealing = result.HasSwitchArg("a");
        _ignoreSingleton = result.HasSwitchArg("i");
        _sourceColor = Rgba32.ParseHex(result.GetRequiredArg("source").StringToHexString());
        _targetColor = Rgba32.ParseHex(result.GetRequiredArg("target").StringToHexString());
        _saveNewFile = result.HasSwitchArg("nf");
        _colorBias = double.Parse(result.GetOptionalArg("bias"));
        Console.WriteLine($"""
                           图片 像素替换工具
                           ==================
                           将颜色{_sourceColor}转为{_targetColor}
                           """);
        foreach (var paramArg in result.GetParamArgs())
            DealPath(paramArg);
        Task.WaitAll(AsyncDealingTasks);
        Console.WriteLine("所有已经识别图片已处理");
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
        foreach (var file in Directory.GetFiles(path)) DealFile(file);

        if (!_enableRecursive) return;
        if (curd > _recursiveDepth) return;
        foreach (var directory in Directory.GetDirectories(path)) DealDir(directory, curd + 1);
    }

    private static void DealFile(string path)
    {
        if (path.Contains(".pixel.")) return;
        if (_enableAsyncDealing)
            AsyncDealingTasks.Add(DealImageAsync(path));
        DealImage(path);
    }

    private static void DealImage(string imagePath)
    {
        try
        {
            using var image = Image.Load(imagePath).CloneAs<Rgba32>();
            Console.WriteLine($"成功加载图片 {Path.GetFileName(imagePath)}({image.Width}x{image.Height})");

            image.ProcessPixelRows(PixelEditAction);

            image.Save(_saveNewFile ? GetNewPath(imagePath) : imagePath, _encoder);
            Console.WriteLine("处理后的图片已保存.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dealing image failed: {imagePath}");
            if (!_ignoreSingleton)
                throw;
        }
    }

    private static void PixelEditAction(PixelAccessor<Rgba32> pixelAccessor)
    {
        try
        {
            for (var r = 0; r < pixelAccessor.Height; r++)
            {
                var rs = pixelAccessor.GetRowSpan(r);

                for (var i = 0; i < rs.Length; i++)
                {
                    var dis = rs[i].CalcCiede2000Difference(_sourceColor);
                    if (dis < _colorBias) rs[i] = _targetColor;
                }
            }
        }
        catch (Exception ex)
        {
        }
    }


    private static async Task DealImageAsync(string imagePath)
    {
        try
        {
            using var image = await Image.LoadAsync(imagePath);
            {
            }
            await image.SaveAsync(GetNewPath(imagePath));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dealing image failed: {imagePath}");
            if (!_ignoreSingleton)
                throw;
        }
    }

    private static void PixelEditActionAsync(PixelAccessor<Rgba32> pixelAccessor)
    {
    }

    private static string GetNewPath(string path)
    {
        var fp = Path.GetFullPath(path);
        return Path.Combine(Path.GetDirectoryName(fp) ?? string.Empty,
            Path.GetFileNameWithoutExtension(fp) + ".pixel.png");
    }
}