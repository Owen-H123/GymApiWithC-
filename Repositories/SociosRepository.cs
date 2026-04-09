using GymAPI.Data;
using GymAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Repositories;

public class SociosRepository : ISociosRepository
{
    private readonly GymDbContext _db;

    public SociosRepository(GymDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Socio>> GetAllAsync()
    {
        return await _db.Socios.Include(s => s.User).ToListAsync();
    }

    public async Task<Socio?> GetByIdAsync(int id)
    {
        return await _db.Socios.Include(s => s.User).FirstOrDefaultAsync(s => s.SocioId == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.NormalizedEmail == email.ToUpper());
    }

    public async Task<Role?> GetSocioRoleAsync()
    {
        return await _db.Roles.FirstOrDefaultAsync(r => r.Name == "SOCIO");
    }

    public async Task<Socio> CreateAsync(Socio socio, User user, string password, int roleId)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole
        {
            UserId = user.UserId,
            RoleId = roleId
        });

        socio.UserId = user.UserId;
        _db.Socios.Add(socio);
        await _db.SaveChangesAsync();

        return socio;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var socio = await _db.Socios.FindAsync(id);
        if (socio == null) return false;

        socio.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }
}
