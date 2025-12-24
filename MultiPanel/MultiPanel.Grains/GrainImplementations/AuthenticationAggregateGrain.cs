using Microsoft.Extensions.Logging;
using MultiPanel.Abstractions.DTOs;
using MultiPanel.Interfaces.IGrains;
using MultiPanel.Shared.Exceptions;
using MultiPanel.Shared.Services;
using Orleans;

namespace MultiPanel.Grains.GrainImplementations;

public class AuthenticationAggregateGrain : Grain, IAuthenticationAggregateGrain
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<AuthenticationAggregateGrain> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationAggregateGrain(
        ILogger<AuthenticationAggregateGrain> logger,
        IPasswordHasher passwordHasher,
        IGrainFactory grainFactory)
    {
        _logger = logger;
        _passwordHasher = passwordHasher;
        _grainFactory = grainFactory;
    }

    public async Task<AuthResponse> Register(UserRegistration registration, string ipAddress, string userAgent)
    {
        try
        {
            _logger.LogInformation("Starting registration for user {Username}", registration.Username);

            // 验证输入
            if (string.IsNullOrEmpty(registration.Username) ||
                string.IsNullOrEmpty(registration.Email) ||
                string.IsNullOrEmpty(registration.Password))
                return AuthResponse.Error("Username, email, and password are required");

            // 检查用户是否已存在
            var userAccountGrain = _grainFactory.GetGrain<IUserAccountGrain>(registration.Username);
            if (await userAccountGrain.Exists()) return AuthResponse.Error("Username already exists");

            // 创建用户
            var passwordHash = _passwordHasher.Hash(registration.Password);
            await userAccountGrain.Initialize(
                registration.Username,
                registration.Email,
                registration.DisplayName,
                passwordHash);

            // 创建会话
            var sessionId = Guid.NewGuid();
            var sessionGrain = _grainFactory.GetGrain<IUserSessionGrain>(sessionId);

            var sessionInfo = await sessionGrain.CreateSession(
                registration.Username,
                registration.Username,
                DateTime.UtcNow.AddHours(2),
                DateTime.UtcNow.AddDays(7),
                ipAddress,
                userAgent);

            // 获取用户信息
            var userInfo = await userAccountGrain.GetUserInfo();
            await userAccountGrain.UpdateLastLogin();

            _logger.LogInformation("User {Username} registered successfully", registration.Username);

            return AuthResponse.Ok(sessionInfo, userInfo, new TokenInfo
            {
                Token = sessionInfo.AccessToken,
                Type = "Bearer",
                IssuedAt = sessionInfo.CreatedAt,
                ExpiresAt = sessionInfo.ExpiresAt,
                RefreshToken = sessionInfo.RefreshToken,
                RefreshTokenExpiresAt = sessionInfo.RefreshTokenExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for user {Username}", registration.Username);
            return AuthResponse.Error($"Registration failed: {ex.Message}");
        }
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user {Username}", request.Username);

            // 获取用户账户
            var userAccountGrain = _grainFactory.GetGrain<IUserAccountGrain>(request.Username);

            // 检查用户是否存在
            if (!await userAccountGrain.Exists())
            {
                await userAccountGrain.IncrementFailedLoginAttempts();
                return AuthResponse.Error("Invalid username or password");
            }

            // 验证密码
            var isValid = await userAccountGrain.ValidatePassword(request.Password);
            if (!isValid) return AuthResponse.Error("Invalid username or password");

            // 检查账户是否激活
            var userInfo = await userAccountGrain.GetUserInfo();
            if (!userInfo.IsActive) return AuthResponse.Error("Account is deactivated");

            // 创建会话
            var sessionId = Guid.NewGuid();
            var sessionGrain = _grainFactory.GetGrain<IUserSessionGrain>(sessionId);

            var expiry = request.RememberMe
                ? DateTime.UtcNow.AddDays(30)
                : // 记住我：30天
                DateTime.UtcNow.AddHours(2); // 普通会话：2小时

            var refreshExpiry = request.RememberMe
                ? DateTime.UtcNow.AddDays(60)
                : // 记住我：60天
                DateTime.UtcNow.AddDays(7); // 普通会话：7天

            var sessionInfo = await sessionGrain.CreateSession(
                userInfo.UserId,
                userInfo.Username,
                expiry,
                refreshExpiry,
                request.IpAddress ?? "unknown",
                request.UserAgent ?? "unknown",
                userInfo.Claims);

            // 更新最后登录时间
            await userAccountGrain.UpdateLastLogin();
            await userAccountGrain.ResetFailedLoginAttempts();

            _logger.LogInformation("User {Username} logged in successfully", request.Username);

            return AuthResponse.Ok(sessionInfo, userInfo, new TokenInfo
            {
                Token = sessionInfo.AccessToken,
                Type = "Bearer",
                IssuedAt = sessionInfo.CreatedAt,
                ExpiresAt = sessionInfo.ExpiresAt,
                RefreshToken = sessionInfo.RefreshToken,
                RefreshTokenExpiresAt = sessionInfo.RefreshTokenExpiresAt
            });
        }
        catch (AuthException ex)
        {
            _logger.LogWarning("Login failed for user {Username}: {Message}",
                request.Username, ex.Message);
            return AuthResponse.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user {Username}", request.Username);
            return AuthResponse.Error($"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthResponse> Logout(Guid sessionId, string accessToken)
    {
        try
        {
            _logger.LogInformation("Logout attempt for session {SessionId}", sessionId);

            var sessionGrain = _grainFactory.GetGrain<IUserSessionGrain>(sessionId);
            var sessionInfo = await sessionGrain.GetSessionInfo();

            // 验证访问令牌
            var isValid = await sessionGrain.ValidateSession(_passwordHasher.Hash(accessToken));
            if (!isValid) return AuthResponse.Error("Invalid session or token");

            // 使会话失效
            await sessionGrain.InvalidateSession();

            _logger.LogInformation("Session {SessionId} logged out successfully", sessionId);

            return AuthResponse.Ok(sessionInfo, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed for session {SessionId}", sessionId);
            return AuthResponse.Error($"Logout failed: {ex.Message}");
        }
    }

    public async Task<AuthResponse> RefreshTokens(Guid sessionId, string refreshToken)
    {
        try
        {
            _logger.LogDebug("Token refresh attempt for session {SessionId}", sessionId);

            var sessionGrain = _grainFactory.GetGrain<IUserSessionGrain>(sessionId);
            var refreshTokenHash = _passwordHasher.Hash(refreshToken);

            // 刷新令牌
            var tokenInfo = await sessionGrain.RefreshTokens(refreshTokenHash);

            // 获取用户信息
            var sessionInfo = await sessionGrain.GetSessionInfo();
            var userAccountGrain = _grainFactory.GetGrain<IUserAccountGrain>(sessionInfo.UserId);
            var userInfo = await userAccountGrain.GetUserInfo();

            _logger.LogInformation("Tokens refreshed for session {SessionId}", sessionId);

            return AuthResponse.Ok(sessionInfo, userInfo, tokenInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed for session {SessionId}", sessionId);
            return AuthResponse.Error($"Token refresh failed: {ex.Message}");
        }
    }

    public async Task<AuthResponse> ValidateSession(ValidateSessionRequest request)
    {
        try
        {
            _logger.LogDebug("Validating session {SessionId}", request.SessionId);

            var sessionGrain = _grainFactory.GetGrain<IUserSessionGrain>(request.SessionId);
            var accessTokenHash = _passwordHasher.Hash(request.AccessToken);

            // 验证会话
            var isValid = await sessionGrain.ValidateSession(accessTokenHash);
            if (!isValid) return AuthResponse.Error("Invalid session or token");

            // 获取会话和用户信息
            var sessionInfo = await sessionGrain.GetSessionInfo();
            var userAccountGrain = _grainFactory.GetGrain<IUserAccountGrain>(sessionInfo.UserId);
            var userInfo = await userAccountGrain.GetUserInfo();

            // 更新活动时间
            await sessionGrain.UpdateActivity();

            return AuthResponse.Ok(sessionInfo, userInfo, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session validation failed for session {SessionId}",
                request.SessionId);
            return AuthResponse.Error($"Session validation failed: {ex.Message}");
        }
    }

    public async Task<List<SessionInfo>> GetUserSessions(string userId)
    {
        var result = new List<SessionInfo>();

        try
        {
            var userAccountGrain = _grainFactory.GetGrain<IUserAccountGrain>(userId);
            var sessionIds = await userAccountGrain.GetActiveSessions();

            foreach (var sessionId in sessionIds)
                try
                {
                    var sessionGrain = _grainFactory.GetGrain<IUserSessionGrain>(sessionId);
                    var sessionInfo = await sessionGrain.GetSessionInfo();
                    result.Add(sessionInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get session {SessionId} for user {UserId}",
                        sessionId, userId);
                }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sessions for user {UserId}", userId);
        }

        return result;
    }

    public async Task<bool> RevokeAllUserSessions(string userId, string reason)
    {
        try
        {
            var sessions = await GetUserSessions(userId);

            foreach (var session in sessions)
                try
                {
                    var sessionGrain = _grainFactory.GetGrain<IUserSessionGrain>(session.SessionId);
                    await sessionGrain.InvalidateSession(reason);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to revoke session {SessionId} for user {UserId}",
                        session.SessionId, userId);
                }

            _logger.LogInformation("All sessions revoked for user {UserId}. Reason: {Reason}",
                userId, reason);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke all sessions for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ForceLogout(Guid sessionId, string reason)
    {
        try
        {
            var sessionGrain = _grainFactory.GetGrain<IUserSessionGrain>(sessionId);
            await sessionGrain.InvalidateSession($"Force logout: {reason}");

            _logger.LogInformation("Session {SessionId} force logged out. Reason: {Reason}",
                sessionId, reason);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to force logout session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> DeactivateUser(string userId, string reason)
    {
        try
        {
            var userAccountGrain = _grainFactory.GetGrain<IUserAccountGrain>(userId);
            await userAccountGrain.Deactivate(reason);

            _logger.LogInformation("User {UserId} deactivated. Reason: {Reason}", userId, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate user {UserId}", userId);
            return false;
        }
    }
}