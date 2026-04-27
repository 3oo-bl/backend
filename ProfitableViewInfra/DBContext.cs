using Microsoft.EntityFrameworkCore;
using ProfitableViewApp.DTOS;

namespace ProfitableViewInfra;

public sealed class DBContext : DbContext
{
    public DbSet<UserDTO> Users { get; set; } = null!;
    public DBContext(DbContextOptions<DBContext> options) : base(options)
    {
        Database.Migrate();
        Console.WriteLine("DB created");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDTO>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();

            entity.HasOne(e => e.Preferences)
                .WithMany()
                .HasForeignKey(e => e.PreferencesId)
                .IsRequired();
        });
    }
}