using Orleans;

namespace MultiPanel.Abstractions.DTOs;

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.DTOs.TokenInfo")]
public class TokenInfo
{
    [Id(0)] public string Token { get; set; } = string.Empty;

    [Id(1)] public string Type { get; set; } = "Bearer";

    [Id(2)] public DateTime IssuedAt { get; set; }

    [Id(3)] public DateTime ExpiresAt { get; set; }

    [Id(4)] public string RefreshToken { get; set; } = string.Empty;

    [Id(5)] public DateTime RefreshTokenExpiresAt { get; set; }
}

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.DTOs.TokenValidationRequest")]
public class TokenValidationRequest
{
    [Id(0)] public string Token { get; set; } = string.Empty;

    [Id(1)] public bool ValidateExpiry { get; set; } = true;
}