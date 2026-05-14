using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Application.Events;

namespace VictoriaIdentityProvider.Application.EventBus.Interfaces
{
    public interface IEventHandler<in TApplicationEvent> where TApplicationEvent:IApplicationEvent
    {
        Task HandleAsync(TApplicationEvent domainEvent);
    }
}
