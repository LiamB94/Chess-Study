using Microsoft.EntityFrameworkCore;

using ChessStudy.Api.Models;

namespace ChessStudy.Api.Data;

public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Position>()
            .HasOne(p => p.ParentPosition)
            .WithMany(p => p.ChildPositions)
            .HasForeignKey(p => p.ParentPositionId)
            .OnDelete(DeleteBehavior.Restrict); 
    }


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<ChessFile> ChessFiles { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Arrow> Arrows { get; set; }

}