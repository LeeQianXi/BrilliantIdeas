using Orleans;

namespace MultiPanel.Abstractions.Entities;

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.Entities.UserEntity")]
public class UserEntity
{
    [Id(0)] public int UserId { get; set; }
    [Id(1)] public string UserName { get; set; } = string.Empty;
    [Id(2)] public string SaltPasswordHash { get; set; } = string.Empty;
    [Id(4)] public List<string> UserRoles { get; } = [];
    [Id(5)] public bool IsActive { get; set; }
    [Id(6)] public DateTime CreatedAt { get; set; }
    [Id(7)] public DateTime UpdatedAt { get; set; }
}