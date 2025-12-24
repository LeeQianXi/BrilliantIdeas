using Microsoft.Extensions.Logging;
using MultiPanel.Abstractions.DTOs;
using MultiPanel.Interfaces.IGrains;
using MultiPanel.Shared.Services;
using Orleans;

namespace MultiPanel.Interfaces.Services;

public interface IAuthenticationService
{
    Task<AuthResponse> RegisterAsync(UserRegistration registration, string ipAddress, string userAgent);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> LogoutAsync(Guid sessionId, string accessToken);
    Task<AuthResponse> RefreshTokensAsync(Guid sessionId, string refreshToken);
    Task<AuthResponse> ValidateSessionAsync(ValidateSessionRequest request);
    Task<UserInfo?> GetUserInfoAsync(string userId);
    Task<List<SessionInfo>> GetUserSessionsAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    Task<bool> UpdateProfileAsync(string userId, string? displayName, string? email);
}

public class AuthenticationService(
    ILogger<AuthenticationService> logger,
    IGrainFactory grainFactory,
    IPasswordHasher passwordHasher)
    : IAuthenticationService
{
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async Task<AuthResponse> RegisterAsync(UserRegistration registration, string ipAddress, string userAgent)
    {
        var aggregateGrainId = Guid.NewGuid();
        var aggregateGrain = grainFactory.GetGrain<IAuthenticationAggregateGrain>(aggregateGrainId);

        return await aggregateGrain.Register(registration, ipAddress, userAgent);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var aggregateGrainId = Guid.NewGuid();
        var aggregateGrain = grainFactory.GetGrain<IAuthenticationAggregateGrain>(aggregateGrainId);

        return await aggregateGrain.Login(request);
    }

    public async Task<AuthResponse> LogoutAsync(Guid sessionId, string accessToken)
    {
        var aggregateGrainId = Guid.NewGuid();
        var aggregateGrain = grainFactory.GetGrain<IAuthenticationAggregateGrain>(aggregateGrainId);

        return await aggregateGrain.Logout(sessionId, accessToken);
    }

    public async Task<AuthResponse> RefreshTokensAsync(Guid sessionId, string refreshToken)
    {
        var aggregateGrainId = Guid.NewGuid();
        var aggregateGrain = grainFactory.GetGrain<IAuthenticationAggregateGrain>(aggregateGrainId);

        return await aggregateGrain.RefreshTokens(sessionId, refreshToken);
    }

    public async Task<AuthResponse> ValidateSessionAsync(ValidateSessionRequest request)
    {
        var aggregateGrainId = Guid.NewGuid();
        var aggregateGrain = grainFactory.GetGrain<IAuthenticationAggregateGrain>(aggregateGrainId);

        return await aggregateGrain.ValidateSession(request);
    }

    public async Task<UserInfo?> GetUserInfoAsync(string userId)
    {
        try
        {
            var userAccountGrain = grainFactory.GetGrain<IUserAccountGrain>(userId);
            return await userAccountGrain.GetUserInfo();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get user info for {UserId}", userId);
            return null;
        }
    }

    public async Task<List<SessionInfo>> GetUserSessionsAsync(string userId)
    {
        var aggregateGrainId = Guid.NewGuid();
        var aggregateGrain = grainFactory.GetGrain<IAuthenticationAggregateGrain>(aggregateGrainId);

        return await aggregateGrain.GetUserSessions(userId);
    }

    public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        try
        {
            var userAccountGrain = grainFactory.GetGrain<IUserAccountGrain>(userId);
            await userAccountGrain.ChangePassword(oldPassword, newPassword);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to change password for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateProfileAsync(string userId, string? displayName, string? email)
    {
        try
        {
            var userAccountGrain = grainFactory.GetGrain<IUserAccountGrain>(userId);
            await userAccountGrain.UpdateProfile(displayName, email);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update profile for user {UserId}", userId);
            return false;
        }
    }
}