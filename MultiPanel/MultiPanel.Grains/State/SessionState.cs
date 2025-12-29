namespace MultiPanel.Grains.State;

/// <summary>
///     会话的内部状态信息
/// </summary>
[GenerateSerializer]
[Alias("MultiPanel.Grains.State.SessionState")]
public class SessionState
{
    [Id(0)] public Guid SessionId { get; set; }

    [Id(1)] public string UserId { get; set; } = string.Empty;

    [Id(2)] public string Username { get; set; } = string.Empty;

    [Id(3)] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Id(4)] public DateTime ExpiresAt { get; set; }

    [Id(5)] public DateTime RefreshTokenExpiresAt { get; set; }

    [Id(6)] public string IpAddress { get; set; } = string.Empty;

    [Id(7)] public string UserAgent { get; set; } = string.Empty;

    [Id(8)] public bool IsActive { get; set; } = true;

    [Id(9)] public Dictionary<string, string> Claims { get; set; } = new();

    [Id(10)] public string AccessTokenHash { get; set; } = string.Empty;

    [Id(11)] public string RefreshTokenHash { get; set; } = string.Empty;

    [Id(12)] public DateTime? RevokedAt { get; set; }

    [Id(13)] public string? RevokedReason { get; set; }
}