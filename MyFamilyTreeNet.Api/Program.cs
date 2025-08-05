using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Api.Services;
using MyFamilyTreeNet.Api.Middleware;
// using MyFamilyTreeNet.Api.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddControllersWithViews(options =>
{
    // Global anti-forgery filter for MVC controllers
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// Configure Anti-Forgery tokens
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
});

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
            connectionString = ConvertRenderDatabaseUrl(connectionString);
        }
        
        options.UseNpgsql(connectionString);
    }
    
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? "ThisIsAVerySecretKeyForWorldFamilyAppMinimum32Characters"))
    };
});

// Configure CORS for Angular app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development - Allow localhost
            policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Production - Allow configured origins and Render.com domains
            var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() 
                ?? new[] { "https://your-frontend-domain.onrender.com" };
            
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "World Family API",
        Version = "v1",
        Description = "Family Social Network & Genealogy Tree API"
    });

    // Configure JWT in Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Register application services
builder.Services.AddScoped<IFamilyService, FamilyService>();
builder.Services.AddScoped<IMemberService, MemberService>();
// TODO: Uncomment when services are implemented
// builder.Services.AddScoped<IRelationshipService, RelationshipService>();
// builder.Services.AddScoped<IPhotoService, PhotoService>();
// builder.Services.AddScoped<IStoryService, StoryService>();

// Register security services
// TODO: Uncomment when security service is implemented
// builder.Services.AddScoped<ISecurityService, MyFamilyTreeNet.Api.Security.SecurityService>();

var app = builder.Build();

static string ConvertRenderDatabaseUrl(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var host = uri.Host;
    var port = uri.Port;
    var database = uri.LocalPath.TrimStart('/');
    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    
    return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.MapGet("/test-404", () => Results.NotFound());
    app.MapGet("/test-500", () => { throw new Exception("Test 500 error"); });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");

    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "World Family API V1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// app.UseMiddleware<GlobalExceptionHandlingMiddleware>(); // Temporarily disabled
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseStaticFiles();

app.UseCors("AllowAngularApp");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "register", 
    pattern: "register",
    defaults: new { controller = "Account", action = "Register" });

app.MapControllerRoute(
    name: "logout",
    pattern: "logout", 
    defaults: new { controller = "Account", action = "Logout" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "errors",
    pattern: "Error/{action=Index}",
    defaults: new { controller = "Error" });
    

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await context.Database.MigrateAsync();

    await SeedData.Initialize(context, userManager, roleManager);
}

app.Run();