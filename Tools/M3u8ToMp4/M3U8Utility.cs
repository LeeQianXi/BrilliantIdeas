namespace M3u8ToMp4;

public static partial class M3U8Utility
{
    [GeneratedRegex("([0-9.]+)")]
    private static partial Regex DurationRegex();

    internal static double ParseDuration(string extInfLine)
    {
        var match = DurationRegex().Match(extInfLine);
        if (match.Success && double.TryParse(match.Groups[1].Value, out var duration)) return duration;

        return 0;
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

    private static async Task<byte[]> DecryptSegmentAsync(byte[] encryptedData, string keyPath)
    {
        var key = await File.ReadAllBytesAsync(keyPath);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.IV = new byte[16]; // 通常 IV 是全零

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encryptedData);
        await using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var result = new MemoryStream();

        await cs.CopyToAsync(result);
        return result.ToArray();
    }

    internal static IEnumerable<byte[]> ReserveSegments(IEnumerable<VideoSegmentInfo> segments)
    {
        foreach (var info in segments)
        {
            if (!File.Exists(info.FilePath))
            {
                Console.WriteLine($"文件不存在: {info.FilePath}");
                continue;
            }

            var data = File.ReadAllBytes(info.FilePath);
            if (!string.IsNullOrEmpty(info.KeyUri) && File.Exists(info.KeyUri))
                data = DecryptSegment(data, info.KeyUri);

            yield return data;
        }
    }

    internal static async IAsyncEnumerable<byte[]> ReserveSegmentsAsync(IEnumerable<VideoSegmentInfo> segments)
    {
        foreach (var info in segments)
        {
            if (!File.Exists(info.FilePath))
            {
                Console.WriteLine($"文件不存在: {info.FilePath}");
                continue;
            }

            var data = await File.ReadAllBytesAsync(info.FilePath);
            if (!string.IsNullOrEmpty(info.KeyUri) && File.Exists(info.KeyUri))
                data = await DecryptSegmentAsync(data, info.KeyUri);

            yield return data;
        }
    }
}