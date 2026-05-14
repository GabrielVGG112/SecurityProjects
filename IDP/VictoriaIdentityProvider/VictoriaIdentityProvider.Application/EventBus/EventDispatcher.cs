using Microsoft.Extensions.DependencyInjection;
using VictoriaIdentityProvider.Application.EventBus.Interfaces;
using VictoriaIdentityProvider.Application.Events;

namespace VictoriaIdentityProvider.Application.EventBus
{
    /// <summary>
    /// Dispatches application events to all registered event handlers that handle the event type.
    /// </summary>
    /// <remarks>Event handlers are resolved from the service provider at the time an event is published. All
    /// handlers for the specified event type will have their handle method invoked. This class is typically used to
    /// decouple event publishers from event consumers within an application.</remarks>
  
    
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        public EventDispatcher(IServiceProvider service)
        {
            _serviceProvider = service;
        }


        /// <summary>
        /// Publishes the specified application event to all registered event handlers that handle the event type.
        /// </summary>
        /// <remarks>All event handlers registered for the specified event type will have their <see
        /// cref="ICustomEventHandler{TApplicationEvent}.Handle"/> method invoked. Handlers are resolved from the service
        /// provider at the time of publishing.</remarks>
        /// <typeparam name="TApplicationEvent">The type of the application event to publish. Must implement <see cref="IApplicationEvent"/>.</typeparam>
        /// <param name="domainEvent">The application event instance to publish to handlers. Cannot be null.</param>
        

        public async Task PublishAsync<TApplicationEvent>(TApplicationEvent domainEvent)
            where TApplicationEvent : IApplicationEvent
        {
           /* IEnumerable<IEventHandler<TApplicationEvent>> */ 
            var handlers = _serviceProvider
                            .GetServices<IEventHandler<TApplicationEvent>>();

            foreach (var handler in handlers) 
            {
               await  handler.HandleAsync(domainEvent);
            }
        }
       
    }
}
