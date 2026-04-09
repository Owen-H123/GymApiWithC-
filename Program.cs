using GymAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── CORS (Necesario para el Frontend Vite/React)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Base de datos
builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT
var jwt = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwt["SecretKey"]!);

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
        ValidIssuer              = jwt["Issuer"],
        ValidAudience            = jwt["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddScoped<GymAPI.Repositories.IAuthRepository, GymAPI.Repositories.AuthRepository>();
builder.Services.AddScoped<GymAPI.Repositories.ISociosRepository, GymAPI.Repositories.SociosRepository>();
builder.Services.AddScoped<GymAPI.Repositories.IEntrenadoresRepository, GymAPI.Repositories.EntrenadoresRepository>();
builder.Services.AddScoped<GymAPI.Repositories.IAsistenciasRepository, GymAPI.Repositories.AsistenciasRepository>();
builder.Services.AddScoped<GymAPI.Repositories.IRutinasRepository, GymAPI.Repositories.RutinasRepository>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Usar política CORS antes de Authentication
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();