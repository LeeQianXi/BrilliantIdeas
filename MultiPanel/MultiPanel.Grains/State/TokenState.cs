namespace MultiPanel.Grains.State;

[GenerateSerializer]
[Alias("MultiPanel.Grains.State.TokenState")]
public class TokenState
{
    /// <summary>
    ///     TokenID
    /// </summary>
    [Id(0)]
    public string TokenId { get; set; } = string.Empty;

    /// <summary>
    ///     UserID
    /// </summary>
    [Id(1)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    ///     会话ID
    /// </summary>
    [Id(2)]
    public Guid SessionId { get; set; }

    /// <summary>
    ///     TokenHash
    /// </summary>
    [Id(3)]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    ///     Token创建时间
    /// </summary>
    [Id(4)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Token过期时间
    /// </summary>
    [Id(5)]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    ///     是否吊销
    /// </summary>
    [Id(6)]
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    ///     吊销时间
    /// </summary>
    [Id(7)]
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    ///     Token种类
    ///     access:访问Token 短期
    ///     refresh:刷新Token 长期
    /// </summary>
    [Id(8)]
    public string TokenType { get; set; } = "access"; // "access" or "refresh"

    /// <summary>
    ///     Token范围
    /// </summary>
    [Id(9)]
    public List<string> Scopes { get; set; } = new();

    /// <summary>
    ///     是否在使用
    /// </summary>
    [Id(10)]
    public bool IsUsed { get; set; } = false;
}