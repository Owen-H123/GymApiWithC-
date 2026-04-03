using GymAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EntrenadoresController : ControllerBase
{
    private readonly GymDbContext _db;

    public EntrenadoresController(GymDbContext db)
    {
        _db = db;
    }

    // GET api/entrenadores — ADMIN y SOCIO
    [HttpGet]
    [Authorize(Roles = "ADMIN,SOCIO")]
    public async Task<IActionResult> GetAll()
    {
        var entrenadores = await _db.Entrenadores
            .Include(e => e.User)
            .Select(e => new {
                e.EntrenadorId,
                e.User.Email,
                e.Especialidad,
                e.Certificaciones,
                e.FechaIngreso,
                e.IsActive
            }).ToListAsync();

        return Ok(entrenadores);
    }

    // GET api/entrenadores/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "ADMIN,ENTRENADOR,SOCIO")]
    public async Task<IActionResult> GetById(int id)
    {
        var entrenador = await _db.Entrenadores
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EntrenadorId == id);

        if (entrenador == null)
            return NotFound(new { mensaje = "Entrenador no encontrado" });

        return Ok(new {
            entrenador.EntrenadorId,
            entrenador.User.Email,
            entrenador.Especialidad,
            entrenador.Certificaciones,
            entrenador.FechaIngreso,
            entrenador.IsActive
        });
    }

    // POST api/entrenadores — Solo ADMIN
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] CrearEntrenadorRequest request)
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

        var rolEntrenador = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "ENTRENADOR");
        if (rolEntrenador != null)
        {
            _db.UserRoles.Add(new GymAPI.Models.UserRole
            {
                UserId = user.UserId,
                RoleId = rolEntrenador.RoleId
            });
        }

        var entrenador = new GymAPI.Models.Entrenadore
        {
            UserId          = user.UserId,
            Especialidad    = request.Especialidad,
            Certificaciones = request.Certificaciones,
            FechaIngreso    = request.FechaIngreso ?? DateOnly.FromDateTime(DateTime.Now)
        };

        _db.Entrenadores.Add(entrenador);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entrenador.EntrenadorId },
            new { mensaje = "Entrenador creado correctamente", entrenador.EntrenadorId });
    }

    // DELETE api/entrenadores/{id} — Solo ADMIN
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var entrenador = await _db.Entrenadores.FindAsync(id);
        if (entrenador == null)
            return NotFound(new { mensaje = "Entrenador no encontrado" });

        entrenador.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Entrenador desactivado correctamente" });
    }
}

public class CrearEntrenadorRequest
{
    public string   Email           { get; set; } = string.Empty;
    public string   Password        { get; set; } = string.Empty;
    public string?  Telefono        { get; set; }
    public string?  Especialidad    { get; set; }
    public string?  Certificaciones { get; set; }
    public DateOnly? FechaIngreso   { get; set; }
}