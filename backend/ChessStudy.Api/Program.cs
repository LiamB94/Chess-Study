using ChessStudy.Api.Data;
using ChessStudy.Api.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Users.Any())
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "dev-only"
        };

        db.Users.Add(user);
        db.SaveChanges();

        db.ChessFiles.Add(new ChessFile
        {
            UserId = user.UserId,
            Name = "Queen's Gambit",
            Description = "Dev seed file"
        });

        db.SaveChanges();
    }
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
// app.UseHttpsRedirection();

app.Run();

