using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Sadalene.Admin.Services;
using Sadalene.Core.Entities.Auth;
using Sadalene.Core.Enums;
using Sadalene.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
});
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Auth/Login";
        options.LogoutPath       = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name      = "Sadalene.Admin";
    });

builder.Services.AddScoped<BarcodeService>();

var app = builder.Build();

// Seed default admin user if Users table is empty
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!db.Users.Any())
    {
        var cfg = app.Configuration.GetSection("AdminSettings");
        db.Users.Add(new User
        {
            FullName     = "Admin",
            Phone        = "9999999999",
            Email        = cfg["DefaultAdminEmail"] ?? "admin@sadalene.com",
            Role         = UserRole.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(cfg["DefaultAdminPassword"] ?? "Admin@1234"),
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapControllers();

app.Run();
