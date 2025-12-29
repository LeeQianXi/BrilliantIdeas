using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MultiPanel.Shared.Services;

public interface ITokenGenerator
{
    TimeSpan RefreshTokenLifeTime { get; }
    TimeSpan AccessTokenLifeTime { get; }

    /// <summary>
    ///     生成RefreshToken,默认3天有效,一次使用过期
    ///     <br />由UUID,userId,设备指纹组成并哈希处理
    /// </summary>
    (string, DateTimeOffset) GeneratorRefreshToken(int userId, TimeSpan ttl);

    (string, DateTimeOffset) GeneratorRefreshToken(int userId);

    /// <summary>
    ///     生成AccessToken,默认15分钟有效,有效期内可一直使用除非吊销
    ///     <br />由<see cref="JsonWebToken" />构成
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="ttl">过期时间</param>
    /// <param name="roles">用户身份组</param>
    string GeneratorAccessToken(int userId, TimeSpan ttl, params IEnumerable<string> roles);

    string GeneratorAccessToken(int userId, params IEnumerable<string> roles);
}

internal class JwtTokenGenerator(IOptions<JwtOption> options) : ITokenGenerator
{
    private readonly SigningCredentials _cred =
        new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Secret)), SecurityAlgorithms.HmacSha256);

    private readonly JwtSecurityTokenHandler _handler = new();

    private JwtOption Option { get; } = options.Value;
    public TimeSpan RefreshTokenLifeTime { get; } = TimeSpan.FromDays(options.Value.RefreshTokenLifetimeDays);
    public TimeSpan AccessTokenLifeTime { get; } = TimeSpan.FromMinutes(options.Value.AccessTokenLifetimeMinutes);

    public (string, DateTimeOffset) GeneratorRefreshToken(int userId, TimeSpan ttl)
    {
        var exp = DateTimeOffset.UtcNow.Add(ttl);
        var payload = new JwtPayload
        {
            ["uid"] = userId,
            ["exp"] = exp.ToUnixTimeSeconds(),
            ["jti"] = Guid.NewGuid().ToString("N") // 随机 id，仅防重放
        };

        var header = new JwtHeader(_cred);
        var token = new JwtSecurityToken(header, payload);
        return (_handler.WriteToken(token), exp);
    }

    public (string, DateTimeOffset) GeneratorRefreshToken(int userId)
    {
        return GeneratorRefreshToken(userId, RefreshTokenLifeTime);
    }

    public string GeneratorAccessToken(int userId, TimeSpan ttl, params IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            Option.Issuer,
            Option.Audience,
            claims,
            expires: DateTime.UtcNow.Add(ttl),
            signingCredentials: _cred);

        return _handler.WriteToken(token);
    }

    public string GeneratorAccessToken(int userId, params IEnumerable<string> roles)
    {
        return GeneratorAccessToken(userId, AccessTokenLifeTime, roles);
    }
}

[Serializable]
public sealed class JwtOption
{
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 3;
}