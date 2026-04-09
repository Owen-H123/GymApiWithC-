using GymAPI.Data;
using GymAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Repositories;

public class RutinasRepository : IRutinasRepository
{
    private readonly GymDbContext _db;

    public RutinasRepository(GymDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Rutina>> GetBySocioIdAsync(int socioId)
    {
        return await _db.Rutinas
            .Include(r => r.Entrenador).ThenInclude(e => e.User)
            .Include(r => r.RutinaEjercicios).ThenInclude(re => re.Ejercicio)
            .Where(r => r.SocioId == socioId)
            .ToListAsync();
    }

    public async Task<Socio?> GetSocioByIdAsync(int socioId)
    {
        return await _db.Socios.FindAsync(socioId);
    }

    public async Task<Rutina> CreateAsync(Rutina rutina)
    {
        _db.Rutinas.Add(rutina);
        await _db.SaveChangesAsync();
        return rutina;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var rutina = await _db.Rutinas.FindAsync(id);
        if (rutina == null) return false;

        rutina.Activa = false;
        await _db.SaveChangesAsync();
        return true;
    }
}
