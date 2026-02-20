using GeneralEditor.Database.Dbe;
using Microsoft.EntityFrameworkCore;

namespace GeneralEditor.Database;

public sealed class GenDbContext(DbContextOptions<GenDbContext> options) : DbContext(options)
{
    public static string DbPath =>
        Path.Combine(Environment.CurrentDirectory, "Data", $"{nameof(GeneralEditor)}.sqlite");

    public DbSet<TechTreeNode> TechTreeNodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TechTreeNodeEntityConfigure());
    }
}