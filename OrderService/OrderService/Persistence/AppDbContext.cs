using Microsoft.EntityFrameworkCore;
using OrderService.Entities;

namespace OrderService.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    // Define DbSets for all desired Entities
    public DbSet<OrderEntity> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(e => e.Id);            
        });

        base.OnModelCreating(modelBuilder);
    }
}
