using Microsoft.EntityFrameworkCore;
using UserService.Entities;

namespace UserService.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {}

    // Define DbSets for all desired Entities
    public DbSet<UserEntity> Users { get; set; }
}
