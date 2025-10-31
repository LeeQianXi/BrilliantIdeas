namespace M3u8ToMp4;

public partial class M3U8Merger
{
    private M3U8Merger(M3U8MergerBuilder builder)
    {
        M3U8FilePath = builder.M3U8FilePath;
        MergeAsync = builder.MergeAsync;
        OutputPath = builder.OutputPath;
        WorkingDirectory = Path.GetDirectoryName(M3U8FilePath) ?? ".";
    }

    private string M3U8FilePath { get; }
    private bool MergeAsync { get; }
    private string OutputPath { get; }
    private string WorkingDirectory { get; }

    [GeneratedRegex("""
                    URI="([^"]*)"
                    """)]
    private static partial Regex UriRegex();

    private List<VideoSegmentInfo> ParseM3U8File()
    {
        var segments = new List<VideoSegmentInfo>();
        var lines = File.ReadAllLines(M3U8FilePath);

        VideoSegmentInfo? currentSegment = null;
        string? keyUri = null;

        foreach (var l in lines)
        {
            var line = l.Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith("#EXTM3U"))
                continue;

            // 解析加密密钥
            if (line.StartsWith("#EXT-X-KEY:"))
            {
                keyUri = ParseKeyUri(line);
                continue;
            }

            // 解析片段信息
            if (line.StartsWith("#EXTINF:"))
            {
                currentSegment = new VideoSegmentInfo
                {
                    Duration = M3U8Utility.ParseDuration(line),
                    KeyUri = keyUri
                };
                continue;
            }

            // 解析片段文件路径
            if ("#".StartsWith(line) || currentSegment == null) continue;
            currentSegment.FilePath = ResolveFilePath(line);
            segments.Add(currentSegment);
            currentSegment = null;
        }

        return segments;
    }

    private string? ParseKeyUri(string extKeyLine)
    {
        var match = UriRegex().Match(extKeyLine);
        if (!match.Success) return null;
        var uri = match.Groups[1].Value;
        return ResolveFilePath(uri);
    }

    private string ResolveFilePath(string filePath)
    {
        // 如果是绝对路径，直接返回
        if (Path.IsPathRooted(filePath))
            return filePath;

        // 如果是 file:// 协议，转换为本地路径
        if (filePath.StartsWith("file://")) filePath = new Uri(filePath).LocalPath;

        // 相对路径转换为绝对路径
        return Path.GetFullPath(Path.Combine(WorkingDirectory, filePath));
    }

    public static M3U8MergerBuilder Builder(string filePath)
    {
        return new M3U8MergerBuilder(filePath);
    }

    public Task Run()
    {
        if (MergeAsync)
            return RunInternalAsync();
        RunInternal();
        return Task.CompletedTask;
    }

    private void RunInternal()
    {
        Console.WriteLine($"开始处理 M3U8 文件: {M3U8FilePath}");

        // 解析 M3U8 文件
        var segments = ParseM3U8File();
        if (segments.Count == 0)
        {
            Console.WriteLine("未找到有效的视频片段");
            throw new MediaNotFoundException(this);
        }

        Console.WriteLine($"找到 {segments.Count} 个视频片段");

        // 合并所有 TS 文件
        {
            using var outputStream = File.Create(OutputPath);
            var totalSegments = segments.Count;
            var step = totalSegments / 10;
            var processedSegments = 0;
            foreach (var reserveSegment in M3U8Utility.ReserveSegments(segments))
            {
                if (totalSegments < 20 || processedSegments % step is 0)
                    Console.WriteLine($"处理片段 {++processedSegments}/{totalSegments}");
                outputStream.Write(reserveSegment, 0, reserveSegment.Length);
            }

            outputStream.Flush();
            outputStream.Close();
        }
        Console.WriteLine($"合并完成! 输出文件: {OutputPath}");
    }

    private async Task RunInternalAsync()
    {
        Console.WriteLine($"开始处理 M3U8 文件: {M3U8FilePath}");

        // 解析 M3U8 文件
        var segments = ParseM3U8File();
        if (segments.Count == 0)
        {
            Console.WriteLine("未找到有效的视频片段");
            throw new MediaNotFoundException(this);
        }

        Console.WriteLine($"找到 {segments.Count} 个视频片段");

        // 合并所有 TS 文件
        {
            await using var outputStream = File.Create(OutputPath);
            var totalSegments = segments.Count;
            var step = totalSegments / 10;
            var processedSegments = 0;
            await foreach (var reserveSegment in M3U8Utility.ReserveSegmentsAsync(segments))
            {
                if (totalSegments < 20 || processedSegments % step is 0)
                    Console.WriteLine($"处理片段 {++processedSegments}/{totalSegments}");
                await outputStream.WriteAsync(reserveSegment);
            }

            outputStream.Flush();
            outputStream.Close();
        }
        Console.WriteLine($"合并完成! 输出文件: {OutputPath}");
    }

    public class M3U8MergerBuilder(string filePath)
    {
        internal readonly string M3U8FilePath = filePath;
        internal bool MergeAsync;

        internal string OutputPath = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty,
            Path.GetFileNameWithoutExtension(filePath) + ".mp4");

        public M3U8MergerBuilder Async()
        {
            MergeAsync = true;
            return this;
        }

        public M3U8MergerBuilder SetOutputPath(string outputPath)
        {
            OutputPath = outputPath;
            return this;
        }

        public M3U8Merger Build()
        {
            return File.Exists(M3U8FilePath)
                ? new M3U8Merger(this)
                : throw new FileNotFoundException($"Can't find {M3U8FilePath}");
        }
    }
}