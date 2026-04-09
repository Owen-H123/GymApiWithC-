using GymAPI.Models;

namespace GymAPI.Repositories;

public interface IAsistenciasRepository
{
    Task<IEnumerable<Asistencia>> GetAllAsync();
    Task<IEnumerable<Asistencia>> GetBySocioIdAsync(int socioId);
    Task<Socio?> GetSocioByIdAsync(int socioId);
    Task<Asistencia> CreateAsync(Asistencia asistencia);
    Task<Asistencia?> GetByIdAsync(int id);
    Task<bool> UpdateSalidaAsync(int id);
}
