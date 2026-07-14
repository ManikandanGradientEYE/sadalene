using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sadalene.API.Services;
using Sadalene.Infrastructure.Data;
using Sadalene.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// EF Core — SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("Sadalene.Infrastructure")
    )
);

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Shared with Sadalene.Admin — same inventory-adjustment logic for both order-placement paths.
builder.Services.AddScoped<OrderInventoryService>();

// OTP (Customer/Agent/Staff login, all via phone) — sent through MSG91 (Msg91Settings:AuthKey /
// TemplateId in config). Swap back to LogOnlyOtpSender for local dev if you don't want to burn real
// SMS credits while testing.
builder.Services.AddHttpClient<IOtpSender, Msg91OtpSender>();
builder.Services.AddScoped<OtpService>();

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ActingCustomerResolver>();
builder.Services.AddScoped<CartLookupService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
