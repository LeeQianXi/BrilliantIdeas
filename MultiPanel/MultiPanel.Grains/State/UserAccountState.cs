using Orleans;

namespace MultiPanel.Grains.State;

[GenerateSerializer]
[Alias("MultiPanel.Grains.State.UserAccountState")]
public class UserAccountState
{
    [Id(0)] public string UserId { get; set; } = string.Empty;

    [Id(1)] public string Username { get; set; } = string.Empty;

    [Id(2)] public string Email { get; set; } = string.Empty;

    [Id(3)] public string DisplayName { get; set; } = string.Empty;

    [Id(4)] public string PasswordHash { get; set; } = string.Empty;

    [Id(5)] public List<string> Roles { get; set; } = new();

    [Id(6)] public Dictionary<string, string> Claims { get; set; } = new();

    [Id(7)] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Id(8)] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Id(9)] public DateTime LastLogin { get; set; } = DateTime.MinValue;

    [Id(10)] public bool IsActive { get; set; } = true;

    [Id(11)] public bool EmailVerified { get; set; } = false;

    [Id(12)] public int FailedLoginAttempts { get; set; } = 0;

    [Id(13)] public DateTime? LockoutEnd { get; set; }

    [Id(14)] public List<Guid> ActiveSessions { get; set; } = new();
}