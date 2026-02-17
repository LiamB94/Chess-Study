using Microsoft.EntityFrameworkCore;

namespace ChessStudy.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // add DbSet<>s here later
}