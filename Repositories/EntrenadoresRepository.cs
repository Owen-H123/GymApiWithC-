using GymAPI.Data;
using GymAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Repositories;

public class EntrenadoresRepository : IEntrenadoresRepository
{
    private readonly GymDbContext _db;

    public EntrenadoresRepository(GymDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Entrenadore>> GetAllAsync()
    {
        return await _db.Entrenadores.Include(e => e.User).ToListAsync();
    }

    public async Task<Entrenadore?> GetByIdAsync(int id)
    {
        return await _db.Entrenadores.Include(e => e.User).FirstOrDefaultAsync(e => e.EntrenadorId == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.NormalizedEmail == email.ToUpper());
    }

    public async Task<Role?> GetEntrenadorRoleAsync()
    {
        return await _db.Roles.FirstOrDefaultAsync(r => r.Name == "ENTRENADOR");
    }

    public async Task<Entrenadore> CreateAsync(Entrenadore entrenador, User user, string password, int roleId)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole
        {
            UserId = user.UserId,
            RoleId = roleId
        });

        entrenador.UserId = user.UserId;
        _db.Entrenadores.Add(entrenador);
        await _db.SaveChangesAsync();

        return entrenador;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var entrenador = await _db.Entrenadores.FindAsync(id);
        if (entrenador == null) return false;

        entrenador.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }
}
