using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.DbConnection;

namespace VictoriaIdentityProvider.Infrastructure.Repositories;

public class RefreshTokenRepository :IRefreshTokenRepository
{
    private readonly VictoriaIdpDbContext _dbContext;
    private readonly IHasher _hasher;
    public RefreshTokenRepository
        (
        VictoriaIdpDbContext dbContext,
        [FromKeyedServices("hmac")]IHasher hasher
        )
    {
        _dbContext = dbContext;
        _hasher = hasher;
    }
    

  public async  Task<RefreshToken?> GetByTokenAsync(string hashedToken) 
    {
        return await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == hashedToken);
    }

    public async Task AddTokenAsync(RefreshToken token) 
    {
     await   _dbContext.RefreshTokens.AddAsync(token);
     await   _dbContext.SaveChangesAsync();
    }

   public async Task<bool> UpdateAsync(RefreshToken token) 
    {
        try
        {
            _dbContext.RefreshTokens.Update(token);

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException) 
        {
            return false;
        }
    }

    public async Task<IEnumerable<RefreshToken>> GetTokensBySessionId(Guid sessionId)
    {
        return await _dbContext.RefreshTokens.AsNoTracking().Where(r=> r.UserSessionId == sessionId).ToListAsync() ;
    }
    public async Task<RefreshToken?> GetByRawTokenAsync(string rawToken) 
    {
        var hash = _hasher.HashData(rawToken);
        return await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == hash);
    }
    public async Task<RefreshToken> GetTokenByIdAsync(Guid rTokenId) 
    {
        return await _dbContext.RefreshTokens.SingleOrDefaultAsync(r => r.Id == rTokenId);
    }

   
}
