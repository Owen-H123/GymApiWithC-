using GymAPI.Models;

namespace GymAPI.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email);
}
