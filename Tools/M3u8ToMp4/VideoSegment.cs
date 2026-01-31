namespace M3u8ToMp4;

public sealed record VideoSegmentInfo
{
    public string? FilePath { get; set; }
    public double Duration { get; set; }
    public string? KeyUri { get; set; }
}