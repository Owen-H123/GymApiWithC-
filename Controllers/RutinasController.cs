using GymAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RutinasController : ControllerBase
{
    private readonly GymDbContext _db;

    public RutinasController(GymDbContext db)
    {
        _db = db;
    }

    // GET api/rutinas/socio/{socioId} — ADMIN, ENTRENADOR, SOCIO
    [HttpGet("socio/{socioId}")]
    [Authorize(Roles = "ADMIN,ENTRENADOR,SOCIO")]
    public async Task<IActionResult> GetBySocio(int socioId)
    {
        var rutinas = await _db.Rutinas
            .Include(r => r.Entrenador).ThenInclude(e => e.User)
            .Include(r => r.RutinaEjercicios).ThenInclude(re => re.Ejercicio)
            .Where(r => r.SocioId == socioId)
            .Select(r => new {
                r.RutinaId,
                r.Nombre,
                r.Objetivo,
                r.FechaInicio,
                r.FechaFin,
                r.Activa,
                entrenador = r.Entrenador != null ? r.Entrenador.User.Email : null,
                ejercicios = r.RutinaEjercicios.Select(re => new {
                    re.Ejercicio.Nombre,
                    re.Series,
                    re.Repeticiones,
                    re.PesoObjetivoKg,
                    re.Orden
                })
            }).ToListAsync();

        return Ok(rutinas);
    }

    // POST api/rutinas — ADMIN y ENTRENADOR
    [HttpPost]
    [Authorize(Roles = "ADMIN,ENTRENADOR")]
    public async Task<IActionResult> Create([FromBody] CrearRutinaRequest request)
    {
        var socio = await _db.Socios.FindAsync(request.SocioId);
        if (socio == null)
            return NotFound(new { mensaje = "Socio no encontrado" });

        var rutina = new GymAPI.Models.Rutina
        {
            SocioId      = request.SocioId,
            EntrenadorId = request.EntrenadorId,
            Nombre       = request.Nombre,
            Objetivo     = request.Objetivo,
            FechaInicio  = request.FechaInicio ?? DateOnly.FromDateTime(DateTime.Now),
            FechaFin     = request.FechaFin
        };

        _db.Rutinas.Add(rutina);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Rutina creada correctamente", rutina.RutinaId });
    }

    // DELETE api/rutinas/{id} — Solo ADMIN
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var rutina = await _db.Rutinas.FindAsync(id);
        if (rutina == null)
            return NotFound(new { mensaje = "Rutina no encontrada" });

        rutina.Activa = false;
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Rutina desactivada correctamente" });
    }
}

public class CrearRutinaRequest
{
    public int      SocioId      { get; set; }
    public int?     EntrenadorId { get; set; }
    public string   Nombre       { get; set; } = string.Empty;
    public string?  Objetivo     { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin    { get; set; }
}