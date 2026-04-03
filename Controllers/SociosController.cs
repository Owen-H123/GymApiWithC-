using GymAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SociosController : ControllerBase
{
    private readonly GymDbContext _db;

    public SociosController(GymDbContext db)
    {
        _db = db;
    }

    // GET api/socios — Solo ADMIN
    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAll()
    {
        var socios = await _db.Socios
            .Include(s => s.User)
            .Select(s => new {
                s.SocioId,
                s.User.Email,
                s.FechaNacimiento,
                s.Genero,
                s.AlturaCm,
                s.PesoKg,
                s.FechaRegistro,
                s.IsActive
            }).ToListAsync();

        return Ok(socios);
    }

    // GET api/socios/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "ADMIN,ENTRENADOR,SOCIO")]
    public async Task<IActionResult> GetById(int id)
    {
        var socio = await _db.Socios
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SocioId == id);

        if (socio == null)
            return NotFound(new { mensaje = "Socio no encontrado" });

        return Ok(new {
            socio.SocioId,
            socio.User.Email,
            socio.FechaNacimiento,
            socio.Genero,
            socio.AlturaCm,
            socio.PesoKg,
            socio.FechaRegistro,
            socio.IsActive
        });
    }

    // POST api/socios — Solo ADMIN
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] CrearSocioRequest request)
    {
        var existeEmail = await _db.Users.AnyAsync(u => u.NormalizedEmail == request.Email.ToUpper());
        if (existeEmail)
            return BadRequest(new { mensaje = "El email ya está registrado" });

        var user = new GymAPI.Models.User
        {
            UserName           = request.Email,
            NormalizedUserName = request.Email.ToUpper(),
            Email              = request.Email,
            NormalizedEmail    = request.Email.ToUpper(),
            PasswordHash       = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber        = request.Telefono
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var rolSocio = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "SOCIO");
        if (rolSocio != null)
        {
            _db.UserRoles.Add(new GymAPI.Models.UserRole
            {
                UserId = user.UserId,
                RoleId = rolSocio.RoleId
            });
        }

        var socio = new GymAPI.Models.Socio
        {
            UserId          = user.UserId,
            FechaNacimiento = request.FechaNacimiento,
            Genero          = request.Genero,
            AlturaCm        = request.AlturaCm,
            PesoKg          = request.PesoKg,
            EmergenciaNombre    = request.EmergenciaNombre,
            EmergenciaTelefono  = request.EmergenciaTelefono
        };

        _db.Socios.Add(socio);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = socio.SocioId },
            new { mensaje = "Socio creado correctamente", socio.SocioId });
    }

    // DELETE api/socios/{id} — Solo ADMIN
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var socio = await _db.Socios.FindAsync(id);
        if (socio == null)
            return NotFound(new { mensaje = "Socio no encontrado" });

        socio.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Socio desactivado correctamente" });
    }
}

public class CrearSocioRequest
{
    public string  Email    { get; set; } = string.Empty;
    public string  Password { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string?   Genero          { get; set; }
    public decimal?  AlturaCm        { get; set; }
    public decimal?  PesoKg          { get; set; }
    public string?   EmergenciaNombre   { get; set; }
    public string?   EmergenciaTelefono { get; set; }
}