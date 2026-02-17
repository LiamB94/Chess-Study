using Microsoft.EntityFrameworkCore;

using ChessStudy.Api.Models;

namespace ChessStudy.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<ChessFile> ChessFiles { get; set; }
}