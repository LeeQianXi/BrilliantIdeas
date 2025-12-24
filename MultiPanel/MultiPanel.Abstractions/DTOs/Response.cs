using Orleans;

namespace MultiPanel.Abstractions.DTOs;

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.DTOs.AuthResponse")]
public class AuthResponse
{
    [Id(0)] public bool Success { get; set; }

    [Id(1)] public string? Message { get; set; }

    [Id(2)] public SessionInfo? Session { get; set; }

    [Id(3)] public UserInfo? User { get; set; }

    [Id(4)] public TokenInfo? Tokens { get; set; }

    public static AuthResponse Ok(SessionInfo session, UserInfo user, TokenInfo tokens)
    {
        return new AuthResponse { Success = true, Session = session, User = user, Tokens = tokens };
    }

    public static AuthResponse Error(string message)
    {
        return new AuthResponse { Success = false, Message = message };
    }
}

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.DTOs.ValidateSessionRequest")]
public class ValidateSessionRequest
{
    [Id(0)] public Guid SessionId { get; set; }

    [Id(1)] public string AccessToken { get; set; } = string.Empty;
}