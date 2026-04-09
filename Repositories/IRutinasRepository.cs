using GymAPI.Models;

namespace GymAPI.Repositories;

public interface IRutinasRepository
{
    Task<IEnumerable<Rutina>> GetBySocioIdAsync(int socioId);
    Task<Socio?> GetSocioByIdAsync(int socioId);
    Task<Rutina> CreateAsync(Rutina rutina);
    Task<bool> DeactivateAsync(int id);
}
