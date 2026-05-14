using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.DbConnection;

namespace VictoriaIdentityProvider.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly VictoriaIdpDbContext _dbContext;
    private readonly IHasher _hasher;
    

    public SessionRepository(
        VictoriaIdpDbContext dbContext,
        [FromKeyedServices("hmac")]IHasher hasher
        )
    {
        _dbContext = dbContext;
        _hasher = hasher;
    }

    public async Task<UserSession?> GetUserSessionById(Guid sessionId)
    {
        return await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }



    public async Task<IEnumerable<UserSession>> GetActiveUserSessionAsync(Guid userId)
    {
        return await _dbContext.Sessions
            .Where(s =>
                s.UserId == userId &&
                s.ExpiresAt > DateTime.UtcNow &&
                s.RevokedAt == null && s.RevokedReason == null
            )
            .ToListAsync();
    }


    public async Task<bool> UpdateUserSessionAsync(UserSession session)
    {
        try
        {
            _dbContext.Sessions.Update(session);
            await _dbContext.SaveChangesAsync();
            return true;
        }catch (DbUpdateConcurrencyException) 
        {
            return false;
        }
    }

    public async Task DeleteUserSessionAsync(UserSession session)
    {
        _dbContext.Sessions.Remove(session);
        await _dbContext.SaveChangesAsync();
    }
    public async Task AddUserSessionAsync(UserSession session) 
    {
        await _dbContext.Sessions.AddAsync(session);
      await  _dbContext.SaveChangesAsync();
    }

    public async Task<UserSession?> GetByTokenAsync(string token)
    {
       return await _dbContext.Sessions.SingleOrDefaultAsync(s=> s.SessionToken == token);
    }
}