using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using NetUtility;

namespace M3u8ToMp4;

public abstract partial class M3U8MergerBase
{
    protected abstract string M3U8FilePath { get; }
    public abstract string OutputPath { get; }
    protected abstract string WorkingDirectory { get; }

    [GeneratedRegex("([0-9.]+)")]
    private static partial Regex DurationRegex();

    /// <summary>
    ///     解析段长
    /// </summary>
    /// <param name="extInfLine"></param>
    /// <returns></returns>
    private static double ParseDuration(string extInfLine)
    {
        var match = DurationRegex().Match(extInfLine);
        if (match.Success && double.TryParse(match.Groups[1].Value, out var duration)) return duration;
        return 0;
    }

    [GeneratedRegex("""
                    URI="([^"]*)"
                    """)]
    private static partial Regex UriRegex();

    /// <summary>
    ///     解析m3u8文件
    /// </summary>
    /// <returns>片段文件信息</returns>
    protected List<VideoSegmentInfo> ParseM3U8File()
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
                    Duration = ParseDuration(line),
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

    /// <summary>
    ///     格式化片段路径
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
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
}

public interface ISyncM3U8Merger
{
    void Merge();
}

internal class M3U8Merger(M3U8MergerBuilder builder) : M3U8MergerBase, ISyncM3U8Merger
{
    protected override string M3U8FilePath => builder.M3U8FilePath;
    public override string OutputPath => builder.OutputPath;
    protected override string WorkingDirectory => Path.GetDirectoryName(M3U8FilePath) ?? ".";

    public void Merge()
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
        using (new DisposableStopWatch(em => Console.WriteLine($"合并完成! 输出文件: {OutputPath} 耗时: {em}ms")))
        {
            var action = CreateMergeAction(segments.Count);
            action.Invoke(OutputPath, segments);
        }

        return;

        static Action<string, IEnumerable<VideoSegmentInfo>> CreateMergeAction(int totalSegments)
        {
            const int targetLogCount = 5;
            // 预计算step，通过捕获值类型避免闭包堆分配
            if (totalSegments <= targetLogCount)
                // 无捕获，可缓存的静态委托（如果totalSegments相同）
                return static (outputPath, segments) =>
                {
                    using var stream = File.Create(outputPath);
                    var i = 0;
                    foreach (var segment in ReserveSegments(segments))
                    {
                        Console.WriteLine($"处理片段 {++i}");
                        stream.Write(segment);
                    }

                    stream.Flush();
                };
            var step = totalSegments / targetLogCount;
            // 仅捕获step（值类型），最小化闭包开销
            return (outputPath, segments) =>
            {
                using var stream = File.Create(outputPath);
                var i = 0;
                foreach (var segment in ReserveSegments(segments))
                {
                    if (i % step == 0) Console.WriteLine($"处理片段 {i + 1}/{totalSegments}");
                    stream.Write(segment);
                    i++;
                }

                stream.Flush();
            };
        }
    }

    private static byte[] DecryptSegment(byte[] encryptedData, string keyPath)
    {
        var key = File.ReadAllBytes(keyPath);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.IV = new byte[16]; // 通常 IV 是全零

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encryptedData);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var result = new MemoryStream();

        cs.CopyTo(result);
        return result.ToArray();
    }

    private static IEnumerable<byte[]> ReserveSegments(IEnumerable<VideoSegmentInfo> segments)
    {
        foreach (var info in segments)
        {
            if (!File.Exists(info.FilePath)) throw new MediaNotFoundException($"文件不存在: {info.FilePath}");

            var data = File.ReadAllBytes(info.FilePath);
            if (!string.IsNullOrEmpty(info.KeyUri) && File.Exists(info.KeyUri))
                data = DecryptSegment(data, info.KeyUri);

            yield return data;
        }
    }
}

public interface IAsyncM3U8Merger
{
    Task Merge(CancellationToken token = default);
}

internal class M3U8MergerAsync(M3U8MergerAsyncBuilder builder) : M3U8MergerBase, IAsyncM3U8Merger
{
    protected override string M3U8FilePath => builder.M3U8FilePath;
    public override string OutputPath => builder.OutputPath;
    protected override string WorkingDirectory => Path.GetDirectoryName(M3U8FilePath) ?? ".";

    public async Task Merge(CancellationToken token = default)
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
        using (new DisposableStopWatch(em => Console.WriteLine($"合并完成! 输出文件: {OutputPath} 耗时: {em}ms")))
        {
            var action = CreateMergeAction(segments.Count);
            await action.Invoke(OutputPath, segments, token);
            token.ThrowIfCancellationRequested();
        }

        return;

        static Func<string, IEnumerable<VideoSegmentInfo>, CancellationToken, Task> CreateMergeAction(int totalSegments)
        {
            const int targetLogCount = 5;
            // 预计算step，通过捕获值类型避免闭包堆分配
            if (totalSegments <= targetLogCount)
                // 无捕获，可缓存的静态委托（如果totalSegments相同）
                return static async (outputPath, segments, token) =>
                {
                    await using var stream = File.Create(outputPath);
                    var i = 0;
                    await foreach (var segment in ReserveSegmentsAsync(segments, token))
                    {
                        Console.WriteLine($"处理片段 {++i}");
                        await stream.WriteAsync(segment, token);
                        token.ThrowIfCancellationRequested();
                    }

                    await stream.FlushAsync(token);
                    token.ThrowIfCancellationRequested();
                };

            var step = totalSegments / targetLogCount;
            // 仅捕获step（值类型），最小化闭包开销
            return async (outputPath, segments, token) =>
            {
                await using var stream = File.Create(outputPath);
                var i = 0;
                await foreach (var segment in ReserveSegmentsAsync(segments, token))
                {
                    if (i % step == 0) Console.WriteLine($"处理片段 {i + 1}/{totalSegments}");
                    await stream.WriteAsync(segment, token);
                    token.ThrowIfCancellationRequested();
                    i++;
                }

                await stream.FlushAsync(token);
                token.ThrowIfCancellationRequested();
            };
        }
    }

    private static async ValueTask<byte[]> DecryptSegmentAsync(byte[] encryptedData, string keyPath,
        CancellationToken token = default)
    {
        var key = await File.ReadAllBytesAsync(keyPath, token);
        token.ThrowIfCancellationRequested();

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.IV = new byte[16]; // 通常 IV 是全零

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encryptedData);
        await using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var result = new MemoryStream();

        await cs.CopyToAsync(result, token);
        token.ThrowIfCancellationRequested();
        return result.ToArray();
    }

    private static async IAsyncEnumerable<ReadOnlyMemory<byte>> ReserveSegmentsAsync(
        IEnumerable<VideoSegmentInfo> segments,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        foreach (var info in segments)
        {
            if (!File.Exists(info.FilePath))
                throw new MediaNotFoundException($"文件不存在: {info.FilePath}");

            var data = await File.ReadAllBytesAsync(info.FilePath, token);
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(info.KeyUri) && File.Exists(info.KeyUri))
            {
                data = await DecryptSegmentAsync(data, info.KeyUri, token);
                token.ThrowIfCancellationRequested();
            }

            yield return data;
        }
    }
}

public class M3U8MergerBuilder(string filePath)
{
    internal readonly string M3U8FilePath = filePath;

    internal string OutputPath = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty,
        Path.GetFileNameWithoutExtension(filePath) + ".mp4");

    public M3U8MergerBuilder SetOutputPath(string outputPath)
    {
        OutputPath = outputPath;
        return this;
    }

    public ISyncM3U8Merger Build()
    {
        return File.Exists(M3U8FilePath)
            ? new M3U8Merger(this)
            : throw new FileNotFoundException($"Can't find {M3U8FilePath}");
    }

    public M3U8MergerAsyncBuilder Async()
    {
        return new M3U8MergerAsyncBuilder(this);
    }
}

public class M3U8MergerAsyncBuilder(M3U8MergerBuilder syncBuilder)
{
    internal readonly string M3U8FilePath = syncBuilder.M3U8FilePath;
    internal string OutputPath = syncBuilder.OutputPath;

    public M3U8MergerAsyncBuilder SetOutputPath(string outputPath)
    {
        OutputPath = outputPath;
        return this;
    }

    public IAsyncM3U8Merger Build()
    {
        return File.Exists(M3U8FilePath)
            ? new M3U8MergerAsync(this)
            : throw new FileNotFoundException($"Can't find {M3U8FilePath}");
    }
}