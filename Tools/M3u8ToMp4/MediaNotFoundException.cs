namespace M3u8ToMp4;

public class MediaNotFoundException : Exception
{
    public MediaNotFoundException(M3U8MergerBase m3U8Merger)
    {
    }

    public MediaNotFoundException()
    {
    }

    public MediaNotFoundException(string? message) : base(message)
    {
    }

    public MediaNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}