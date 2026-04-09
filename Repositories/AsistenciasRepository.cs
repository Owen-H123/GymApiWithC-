using GymAPI.Data;
using GymAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Repositories;

public class AsistenciasRepository : IAsistenciasRepository
{
    private readonly GymDbContext _db;

    public AsistenciasRepository(GymDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Asistencia>> GetAllAsync()
    {
        return await _db.Asistencias
            .Include(a => a.Socio).ThenInclude(s => s.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Asistencia>> GetBySocioIdAsync(int socioId)
    {
        return await _db.Asistencias
            .Include(a => a.Socio).ThenInclude(s => s.User)
            .Where(a => a.SocioId == socioId)
            .ToListAsync();
    }

    public async Task<Socio?> GetSocioByIdAsync(int socioId)
    {
        return await _db.Socios.FindAsync(socioId);
    }

    public async Task<Asistencia> CreateAsync(Asistencia asistencia)
    {
        _db.Asistencias.Add(asistencia);
        await _db.SaveChangesAsync();
        return asistencia;
    }

    public async Task<Asistencia?> GetByIdAsync(int id)
    {
        return await _db.Asistencias.FindAsync(id);
    }

    public async Task<bool> UpdateSalidaAsync(int id)
    {
        var asistencia = await _db.Asistencias.FindAsync(id);
        if (asistencia == null || asistencia.FechaHoraSalida != null) return false;

        asistencia.FechaHoraSalida = DateTime.Now;
        await _db.SaveChangesAsync();
        return true;
    }
}
