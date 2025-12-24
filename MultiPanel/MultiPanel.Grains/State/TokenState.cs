using Orleans;

namespace MultiPanel.Grains.State;

[GenerateSerializer]
[Alias("MultiPanel.Grains.State.TokenState")]
public class TokenState
{
    [Id(0)] public string TokenId { get; set; } = string.Empty;

    [Id(1)] public string UserId { get; set; } = string.Empty;

    [Id(2)] public Guid SessionId { get; set; }

    [Id(3)] public string TokenHash { get; set; } = string.Empty;

    [Id(4)] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Id(5)] public DateTime ExpiresAt { get; set; }

    [Id(6)] public bool IsRevoked { get; set; } = false;

    [Id(7)] public DateTime? RevokedAt { get; set; }

    [Id(8)] public string TokenType { get; set; } = "access"; // "access" or "refresh"

    [Id(9)] public List<string> Scopes { get; set; } = new();

    [Id(10)] public bool IsUsed { get; set; } = false;
}