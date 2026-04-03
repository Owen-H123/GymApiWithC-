using GymAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GymAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GymDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(GymDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == request.Email.ToUpper());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { mensaje = "Credenciales incorrectas" });

        var rol = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "SOCIO";
        var token = GenerarToken(user.UserId, user.Email, rol);

        return Ok(new
        {
            token,
            email = user.Email,
            rol,
            expiracion = DateTime.UtcNow.AddHours(8)
        });
    }
    [HttpPost("setup")]
public async Task<IActionResult> Setup()
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
    if (user == null) return NotFound();
    
    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
    await _db.SaveChangesAsync();
    
    return Ok(new { mensaje = "Password actualizado correctamente" });
}

    private string GenerarToken(int userId, string email, string rol)
    {
        var jwt = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, rol)
        };

        var token = new JwtSecurityToken(
            issuer:    jwt["Issuer"],
            audience:  jwt["Audience"],
            claims:    claims,
            expires:   DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Email, string Password);