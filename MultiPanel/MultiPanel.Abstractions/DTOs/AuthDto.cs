using Orleans;

namespace MultiPanel.Abstractions.DTOs;

/// <summary>
///     注册/登录/刷新 统一返回体
/// </summary>
[GenerateSerializer]
[Alias("MultiPanel.Abstractions.DTOs.AuthDto")]
public sealed class AuthDto
{
    [Id(0)] public int UserId { get; set; }
    [Id(1)] public string AccessToken { get; set; } = default!;
    [Id(2)] public string RefreshToken { get; init; } = default!;
    [Id(3)] public DateTimeOffset ExpiresAt { get; init; }
    public bool IsValid => !(string.IsNullOrWhiteSpace(AccessToken) || string.IsNullOrWhiteSpace(RefreshToken));
}