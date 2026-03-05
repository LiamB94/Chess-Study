using ChessStudy.Api.Data;
using ChessStudy.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------- JWT CONFIG (FIXED) --------------------
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// If Jwt:Key is missing, we use a dev fallback in Development to avoid crashing.
// In non-dev environments, we fail fast.
if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtKey = "DEV_ONLY_SUPER_LONG_SECRET_KEY_AT_LEAST_32_CHARS_1234567890";
        Console.WriteLine("WARNING: Jwt:Key is missing. Using a DEV-ONLY fallback key. Set Jwt:Key in appsettings.Development.json.");
    }
    else
    {
        throw new InvalidOperationException("Missing Jwt:Key. Set Jwt:Key in appsettings.json/appsettings.Production.json or environment variables.");
    }
}

// Provide sensible defaults for issuer/audience if not set.
jwtIssuer ??= "ChessStudy";
jwtAudience ??= "ChessStudy";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
    );
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

// -------------------- DEV SEED --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Users.Any())
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "dev-only" // NOTE: for real auth, hash this properly
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

// OpenAPI (Development only)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
// app.UseHttpsRedirection();

app.Run();