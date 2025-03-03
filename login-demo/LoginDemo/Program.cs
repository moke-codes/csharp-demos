using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LoginDemo.Data;
using LoginDemo.Models;
using LoginDemo.Services;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Configure Entity Framework Core with SQLite database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Configure token providers
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
    options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultProvider;
    options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultProvider;
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["JWT:Secret"];
var jwtIssuer = builder.Configuration["JWT:ValidIssuer"];
var jwtAudience = builder.Configuration["JWT:ValidAudience"];

if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is missing. Please check appsettings.json");
}

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// Register AuthService and EmailService
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IEmailSender<ApplicationUser>, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configure rate limiting middleware
app.UseIpRateLimiting();

// Configure static files
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add explicit routes for HTML pages
app.MapGet("/reset-password", () => Results.File(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reset-password.html"), "text/html"));
app.MapGet("/register", () => Results.File(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "register.html"), "text/html"));
app.MapGet("/forgot-password", () => Results.File(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "forgot-password.html"), "text/html"));

// Add default route for the root path
app.MapFallbackToFile("index.html");

// Seed the database with a test user
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Create roles if they don't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Create a test user
    var testUser = await userManager.FindByEmailAsync("test@example.com");
    if (testUser == null)
    {
        testUser = new ApplicationUser
        {
            UserName = "test@example.com",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var result = await userManager.CreateAsync(testUser, "Test123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(testUser, "Admin");
        }
    }
}

app.Run();
