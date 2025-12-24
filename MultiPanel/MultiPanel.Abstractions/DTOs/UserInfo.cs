using Orleans;

namespace MultiPanel.Abstractions.DTOs;

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.DTOs.UserInfo")]
public class UserInfo
{
    [Id(0)] public string UserId { get; set; } = string.Empty;

    [Id(1)] public string Username { get; set; } = string.Empty;

    [Id(2)] public string Email { get; set; } = string.Empty;

    [Id(3)] public string DisplayName { get; set; } = string.Empty;

    [Id(4)] public List<string> Roles { get; set; } = new();

    [Id(5)] public Dictionary<string, string> Claims { get; set; } = new();

    [Id(6)] public DateTime CreatedAt { get; set; }

    [Id(7)] public DateTime LastLogin { get; set; }

    [Id(8)] public bool IsActive { get; set; } = true;

    [Id(9)] public bool EmailVerified { get; set; } = false;
}

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.DTOs.UserRegistration")]
public class UserRegistration
{
    [Id(0)] public string Username { get; set; } = string.Empty;

    [Id(1)] public string Email { get; set; } = string.Empty;

    [Id(2)] public string Password { get; set; } = string.Empty;

    [Id(3)] public string DisplayName { get; set; } = string.Empty;
}