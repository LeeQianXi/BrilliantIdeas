using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneralEditor.Database.Dbe;

public class TechTreeNode
{
    public required Guid NodeId { get; init; }

    [MaxLength(100)] public required string Name { get; init; }
}

public sealed class TechTreeNodeEntityConfigure : IEntityTypeConfiguration<TechTreeNode>
{
    public void Configure(EntityTypeBuilder<TechTreeNode> builder)
    {
        builder.ToTable("TechTreeNode");
        builder.HasKey(e => e.NodeId)
            .HasName("PK_TechTreeNode_NodeId");

        builder.Property(e => e.Name)
            .HasColumnType("varchar(100)")
            .IsRequired()
            .HasColumnName("NodeName")
            .ValueGeneratedNever();

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_TechTreeNode_NodeName")
            .IsUnique();
    }
}