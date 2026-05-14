using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Events
{
    public record LoginEvent(string EventType,
    string Message,
    Guid UserId,
    string Ip,
    string UserAgent,
    int FailedLoginAttempts,
    DateTime OccuredAt,
    Guid? SessionId = null,
    Guid? TokenId = null) : IApplicationEvent;

}
