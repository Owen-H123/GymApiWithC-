using GymAPI.Models;

namespace GymAPI.Repositories;

public interface IEntrenadoresRepository
{
    Task<IEnumerable<Entrenadore>> GetAllAsync();
    Task<Entrenadore?> GetByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<Role?> GetEntrenadorRoleAsync();
    Task<Entrenadore> CreateAsync(Entrenadore entrenador, User user, string password, int roleId);
    Task<bool> DeactivateAsync(int id);
}
