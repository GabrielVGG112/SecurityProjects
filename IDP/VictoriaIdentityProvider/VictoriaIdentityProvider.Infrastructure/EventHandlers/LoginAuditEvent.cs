using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Application.EventBus.Interfaces;
using VictoriaIdentityProvider.Application.Events;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.DbConnection;

namespace VictoriaIdentityProvider.Infrastructure.EventHandlers
{
    public class LoginAuditEvent : IEventHandler<LoginEvent>
    {
        private readonly VictoriaIdpDbContext _context;

        public LoginAuditEvent(VictoriaIdpDbContext context)
        {
            _context = context;
        }
        public async Task HandleAsync(LoginEvent e)
        {
            var log = new AuditLog
            {
                EventType = e.EventType,
                UserId = e.UserId,
                Ip = e.Ip,
                UserAgent = e.UserAgent,
                SessionId = e.SessionId,
                TokenId = e.TokenId,
                Message = e.Message,
                CreatedAtUtc = e.OccuredAt
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
