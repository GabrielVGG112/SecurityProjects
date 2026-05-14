using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Application.Events;

namespace VictoriaIdentityProvider.Application.EventBus.Interfaces
{
    public interface IEventDispatcher
    {
        Task PublishAsync<TApplicationEvent>(TApplicationEvent domainEvent)
            where TApplicationEvent : IApplicationEvent;
    }
}