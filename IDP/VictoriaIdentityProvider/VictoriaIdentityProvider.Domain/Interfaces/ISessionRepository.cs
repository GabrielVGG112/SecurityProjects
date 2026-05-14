using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Domain.Interfaces
{
    public interface ISessionRepository
    {
       
        Task<UserSession?>GetByTokenAsync(string token);
        Task<UserSession?> GetUserSessionById(Guid sessionId);
        Task AddUserSessionAsync(UserSession userSession);
        Task<IEnumerable<UserSession>> GetActiveUserSessionAsync(Guid userId);
        Task<bool> UpdateUserSessionAsync(UserSession session);
        Task DeleteUserSessionAsync(UserSession session);
    }
}
