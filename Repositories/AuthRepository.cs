using GymAPI.Data;
using GymAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly GymDbContext _db;

    public AuthRepository(GymDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
    }
}
