using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using SnakeGame.Server.Models;
namespace SnakeGame.Server.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) :
    base(options)
    { }
    public DbSet<Player> Players => Set<Player>();
    public DbSet<GameResult> GameResults => Set<GameResult>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>().HasIndex(p => p.Name).IsUnique(false);
        base.OnModelCreating(modelBuilder);
    }
}