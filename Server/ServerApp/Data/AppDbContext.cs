using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ActionRecord> Actions => Set<ActionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Налаштування зв'язку Users -> Items (Каскадне видалення)
        modelBuilder.Entity<Item>()
            .HasOne(i => i.User)
            .WithMany(u => u.Items)
            .HasForeignKey(i => i.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // Налаштування зв'язку Users -> Actions
        modelBuilder.Entity<ActionRecord>()
            .HasOne(a => a.User)
            .WithMany(u => u.Actions)
            .HasForeignKey(a => a.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // Налаштування зв'язку Items -> Actions
        modelBuilder.Entity<ActionRecord>()
            .HasOne(a => a.Item)
            .WithMany(i => i.Actions)
            .HasForeignKey(a => a.ItemID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}