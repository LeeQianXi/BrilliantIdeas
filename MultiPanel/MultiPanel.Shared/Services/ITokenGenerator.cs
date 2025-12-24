using System.Text;
using System.Text.Json;

namespace MultiPanel.Shared.Services;

public interface ITokenGenerator
{
    string GenerateToken(string userId, Guid sessionId, DateTime expiry, string tokenType, List<string>? scopes = null);
    string GenerateJwtToken(string userId, Dictionary<string, object> claims, DateTime expiry);
    (string userId, Dictionary<string, object> claims) ValidateJwtToken(string token);
}

public class SimpleTokenGenerator : ITokenGenerator
{
    private readonly Random _random = new();

    public string GenerateToken(string userId, Guid sessionId, DateTime expiry, string tokenType,
        List<string>? scopes = null)
    {
        // 简单实现：生成随机会话令牌
        var bytes = new byte[32];
        _random.NextBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public string GenerateJwtToken(string userId, Dictionary<string, object> claims, DateTime expiry)
    {
        // 在实际项目中，这里应该使用JWT库生成JWT令牌
        // 这只是简单实现
        var header = Convert.ToBase64String("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"u8.ToArray());

        claims["sub"] = userId;
        claims["exp"] = new DateTimeOffset(expiry).ToUnixTimeSeconds();
        claims["iat"] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(claims)));

        return $"{header}.{payload}.signature";
    }

    public (string userId, Dictionary<string, object> claims) ValidateJwtToken(string token)
    {
        // 在实际项目中，这里应该验证JWT签名
        // 这只是简单实现
        var parts = token.Split('.');
        if (parts.Length != 3)
            throw new Exception("Invalid token format");

        var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
        var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson)
                     ?? new Dictionary<string, object>();

        var userId = claims.TryGetValue("sub", out var sub) ? sub.ToString() : string.Empty;

        return (userId ?? string.Empty, claims);
    }
}