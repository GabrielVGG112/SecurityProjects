using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<IEnumerable<RefreshToken>> GetTokensBySessionId(Guid sessionId);
        Task<RefreshToken?> GetByTokenAsync(string hashedToken);
        Task AddTokenAsync(RefreshToken token);
         Task<bool> UpdateAsync(RefreshToken token);
        Task<RefreshToken?> GetByRawTokenAsync(string rawToken);
        Task<RefreshToken?> GetTokenByIdAsync(Guid rTokenId);
    }
}
