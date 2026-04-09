using GymAPI.Models;

namespace GymAPI.Repositories;

public interface ISociosRepository
{
    Task<IEnumerable<Socio>> GetAllAsync();
    Task<Socio?> GetByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<Socio> CreateAsync(Socio socio, User user, string password, int roleId);
    Task<Role?> GetSocioRoleAsync();
    Task<bool> DeactivateAsync(int id);
}
