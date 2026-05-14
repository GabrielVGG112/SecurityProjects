using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.Factory
{
    public class ModelFactoryFacade
    {
        private static Func<User, (IUserDependency, string)> Wrap<T>(Func<User, (T dep, string rawToken)> factory)
            where T : IUserDependency
        
        {                                                                 
            return user => 
            {
                var (dep, rawToken) = factory(user);
               
                        // for clarity
                return ((IUserDependency)dep, rawToken);
            };
        }
        private readonly Dictionary<InstanceNamesEnum, Func<User, (IUserDependency dep,string dto)>> _caller = new();
        protected ModelFactoryFacade() { }
        public ModelFactoryFacade(SessionFactory sessionFactory , TokenFactory refreshTokenFactory)
        {
            _caller[InstanceNamesEnum.RefreshToken] = Wrap(refreshTokenFactory.CreateModel);
            _caller[InstanceNamesEnum.UserSession] = Wrap(sessionFactory.CreateModel);
         }
        public virtual (T,string)Create<T>(InstanceNamesEnum key, User user) where T : IUserDependency
        {
            if (!_caller.TryGetValue(key, out var factory))
                throw new InvalidOperationException($"Factory not found: {key}");
             var (dep,dto) = factory(user);

            return ((T)dep, dto);
        }
    }
}

