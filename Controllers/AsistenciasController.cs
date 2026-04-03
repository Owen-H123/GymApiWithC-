using GymAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AsistenciasController : ControllerBase
{
    private readonly GymDbContext _db;

    public AsistenciasController(GymDbContext db)
    {
        _db = db;
    }

    // GET api/asistencias — Solo ADMIN
    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAll()
    {
        var asistencias = await _db.Asistencias
            .Include(a => a.Socio).ThenInclude(s => s.User)
            .Select(a => new {
                a.AsistenciaId,
                socio = a.Socio.User.Email,
                a.FechaHoraEntrada,
                a.FechaHoraSalida,
                a.Observaciones
            }).ToListAsync();

        return Ok(asistencias);
    }

    // GET api/asistencias/socio/{socioId} — ADMIN, ENTRENADOR o el mismo SOCIO
    [HttpGet("socio/{socioId}")]
    [Authorize(Roles = "ADMIN,ENTRENADOR,SOCIO")]
    public async Task<IActionResult> GetBySocio(int socioId)
    {
        var asistencias = await _db.Asistencias
            .Include(a => a.Socio).ThenInclude(s => s.User)
            .Where(a => a.SocioId == socioId)
            .Select(a => new {
                a.AsistenciaId,
                a.FechaHoraEntrada,
                a.FechaHoraSalida,
                a.Observaciones
            }).ToListAsync();

        return Ok(asistencias);
    }

    // POST api/asistencias — ADMIN y ENTRENADOR
    [HttpPost]
    [Authorize(Roles = "ADMIN,ENTRENADOR")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarAsistenciaRequest request)
    {
        var socio = await _db.Socios.FindAsync(request.SocioId);
        if (socio == null)
            return NotFound(new { mensaje = "Socio no encontrado" });

        var asistencia = new GymAPI.Models.Asistencia
        {
            SocioId          = request.SocioId,
            FechaHoraEntrada = DateTime.Now,
            Observaciones    = request.Observaciones,
            RegistradaPorUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
        };

        _db.Asistencias.Add(asistencia);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Asistencia registrada correctamente", asistencia.AsistenciaId });
    }

    // PUT api/asistencias/{id}/salida — ADMIN y ENTRENADOR
    [HttpPut("{id}/salida")]
    [Authorize(Roles = "ADMIN,ENTRENADOR")]
    public async Task<IActionResult> RegistrarSalida(int id)
    {
        var asistencia = await _db.Asistencias.FindAsync(id);
        if (asistencia == null)
            return NotFound(new { mensaje = "Asistencia no encontrada" });

        if (asistencia.FechaHoraSalida != null)
            return BadRequest(new { mensaje = "La salida ya fue registrada" });

        asistencia.FechaHoraSalida = DateTime.Now;
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Salida registrada correctamente" });
    }
}

public class RegistrarAsistenciaRequest
{
    public int     SocioId       { get; set; }
    public string? Observaciones { get; set; }
}