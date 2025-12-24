using System.Text.Json.Serialization;
using Orleans;

namespace MultiPanel.Abstractions.DTOs;

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.DTOs.SessionInfo")]
public class SessionInfo
{
    [Id(0)] public Guid SessionId { get; set; }

    [Id(1)] public string UserId { get; set; } = string.Empty;

    [Id(2)] public string Username { get; set; } = string.Empty;

    [Id(3)] public DateTime CreatedAt { get; set; }

    [Id(4)] public DateTime ExpiresAt { get; set; }

    [Id(5)] public string AccessToken { get; set; } = string.Empty;

    [Id(6)] public string RefreshToken { get; set; } = string.Empty;

    [Id(7)] public DateTime RefreshTokenExpiresAt { get; set; }

    [Id(8)] public Dictionary<string, string> Claims { get; set; } = new();

    [Id(9)] public string IpAddress { get; set; } = string.Empty;

    [Id(10)] public string UserAgent { get; set; } = string.Empty;

    [Id(11)] public bool IsActive { get; set; } = true;

    [JsonIgnore] public bool IsValid => IsActive && DateTime.UtcNow < ExpiresAt;

    [JsonIgnore] public bool CanRefresh => IsActive && DateTime.UtcNow < RefreshTokenExpiresAt;
}

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.DTOs.LoginRequest")]
public class LoginRequest
{
    [Id(0)] public string Username { get; set; } = string.Empty;

    [Id(1)] public string Password { get; set; } = string.Empty;

    [Id(2)] public bool RememberMe { get; set; } = false;

    [Id(3)] public string? IpAddress { get; set; }

    [Id(4)] public string? UserAgent { get; set; }
}