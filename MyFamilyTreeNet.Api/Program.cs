using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework with SQLite (Development) or PostgreSQL (Production)
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider");
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (isDevelopment && databaseProvider == "SQLite")
    {
        // Development - SQLite
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else
    {
        // Production - PostgreSQL
        var connectionString = builder.Configuration.GetConnectionString("PostgreSQLConnection") 
            ?? Environment.GetEnvironmentVariable("DATABASE_URL");
            
        if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres://"))
        {
            // Parse Render.com DATABASE_URL format
            var uri = new Uri(connectionString);
            var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Username = uri.UserInfo.Split(':')[0],
                Password = uri.UserInfo.Split(':')[1],
                Database = uri.LocalPath.Substring(1),
                SslMode = Npgsql.SslMode.Require
            };
            connectionString = npgsqlBuilder.ConnectionString;
        }
        
        options.UseNpgsql(connectionString);
    }
});

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] 
    ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? "ThisIsAVerySecretKeyForMyFamilyTreeNetAppMinimum32Characters";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "MyFamilyTreeNetApi",
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "MyFamilyTreeNetClients",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

var corsOrigins = builder.Configuration["CORS:AllowedOrigins"]?.Split(',')
    ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")?.Split(',')
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyFamilyTreeNetPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("MyFamilyTreeNetPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await context.Database.MigrateAsync();

    await SeedData.Initialize(context, userManager, roleManager);
}

app.Run();