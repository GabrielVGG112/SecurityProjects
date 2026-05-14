using Microsoft.EntityFrameworkCore;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.DbConnection;

namespace VictoriaIdentityProvider.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly VictoriaIdpDbContext _context;

    public UserRepository(VictoriaIdpDbContext context)
    {
        _context = context;
    }

    public async Task AddUserAsync(User user)
    {


        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
       return user;
    }

    public async Task<bool> VerifyIfEmailExistsAsync(string email)
    {
        return await _context.Users
              .AsNoTracking()
              .AnyAsync(u => u.Email == email);
    }
    public async Task<bool> VerifyIfIdExistsAsync(Guid id)
    {
        return await _context.Users
              .AsNoTracking()
              .AnyAsync(u => u.Id == id);
    }
    public async Task UpdateUserAsync(User user)
    {
 
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    public async Task<User>GetUserWithNavigationsById(Guid id) 
    {
        var user = await _context.Users.Include(u=>u.RefreshTokens).ThenInclude(r=>r.UserSession).SingleOrDefaultAsync(u => u.Id == id);

        return user;
    }
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

        return user;
    }
}

