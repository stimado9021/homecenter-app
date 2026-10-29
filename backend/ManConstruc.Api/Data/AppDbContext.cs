using ManConstruc.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ManConstruc.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Work> Works => Set<Work>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<MaterialLine> MaterialLines => Set<MaterialLine>();
    public DbSet<LaborLine> LaborLines => Set<LaborLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(180).IsRequired();
            entity.Property(u => u.FullName).HasMaxLength(160).IsRequired();
            entity.Property(u => u.CompanyName).HasMaxLength(160);
            entity.Property(u => u.PasswordHash).HasMaxLength(400).IsRequired();
            entity.HasMany(u => u.Works).WithOne(w => w.User!).HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Work>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Name).HasMaxLength(160).IsRequired();
            entity.Property(w => w.ClientName).HasMaxLength(160);
            entity.Property(w => w.Location).HasMaxLength(200);
            entity.Property(w => w.Description).HasMaxLength(1000);
            entity.Property(w => w.Currency).HasMaxLength(8).IsRequired();
            entity.HasMany(w => w.Items).WithOne(i => i.Work!).HasForeignKey(i => i.WorkId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Name).HasMaxLength(200).IsRequired();
            entity.Property(i => i.Unit).HasMaxLength(24).IsRequired();
            entity.Property(i => i.Notes).HasMaxLength(500);
            entity.HasMany(i => i.Materials).WithOne(m => m.WorkItem!).HasForeignKey(m => m.WorkItemId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(i => i.Labors).WithOne(l => l.WorkItem!).HasForeignKey(l => l.WorkItemId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MaterialLine>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Description).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Unit).HasMaxLength(24).IsRequired();
        });

        modelBuilder.Entity<LaborLine>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Description).HasMaxLength(200).IsRequired();
            entity.Property(l => l.Unit).HasMaxLength(24).IsRequired();
        });
    }
}
