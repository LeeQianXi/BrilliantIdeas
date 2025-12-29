using Orleans;

namespace MultiPanel.Abstractions.Entities;

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.Entities.RoleEntity")]
public class RoleEntity
{
    [Id(0)] public int RoleId { get; set; }
    [Id(1)] public string RoleName { get; set; } = string.Empty;
    [Id(2)] public string Description { get; set; } = string.Empty;
    [Id(3)] public DateTime CreatedAt { get; set; }
}