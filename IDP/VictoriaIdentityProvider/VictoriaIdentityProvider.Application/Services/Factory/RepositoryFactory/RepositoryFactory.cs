using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.Factory.RepositoryFactory
{

    // needs to be refactored
    public class RepositoryFactory : IRepositoryFactory
    {
        private static Func<IUserDependency, Task<bool>> WrapUpdater<T>(Func<T, Task<bool>> update) where T : IUserDependency

           => async udep => await update((T)udep);

        private static Func<IUserDependency, Task> WrapAdder<T>(Func<T, Task> add) where T : IUserDependency
        {
            return async dependency => await add((T)dependency);

        }

        private static Func<Guid, Task<IUserDependency>> WrapGetById<T>(Func<Guid, Task<T?>> getByIdAsync) where T : IUserDependency
        {
            return async id => 
            {
               var dependency = await getByIdAsync(id);
                if (dependency is not T)
                    throw new InvalidOperationException($"Invalid operation {dependency?.GetType().Name} should be {typeof(T)}");

                return dependency;
            };

        }

        private static Func<string, Task<IUserDependency>> WrapGetByToken<T>(Func<string, Task<T?>> getByToken) where T : IUserDependency
        {
            return async rawToken => 
            {
              var dependency =  await getByToken(rawToken);
                if (dependency is not T) 
                    throw new InvalidOperationException($"Invalid operation {dependency?.GetType().Name} should be {typeof(T)}");

                return dependency;
            };
        }
        private static Func<Guid, Task<IEnumerable<IUserDependency>>> WrapGetMoreDependencyes<T>(Func<Guid, Task<IEnumerable<T>>> getMoreDependecies) where T : IUserDependency 
        {
         return async specificId => 
         { 
             var list = await getMoreDependecies(specificId);
             return list.Cast<IUserDependency>();
         };
        }
        private readonly Dictionary<InstanceNamesEnum, Func<IUserDependency, Task<bool>>> _updateRepos;
        private readonly Dictionary<InstanceNamesEnum, Func<IUserDependency, Task>> _addRepos;
        private readonly Dictionary<InstanceNamesEnum, Func<Guid, Task<IUserDependency>>> _getById;
        private readonly Dictionary<InstanceNamesEnum, Func<string, Task<IUserDependency>>> _getByToken;

        private readonly Dictionary<InstanceNamesEnum, Func<Guid, Task<IEnumerable<IUserDependency>>>> _getMultipleDependecies;
        public RepositoryFactory(IRefreshTokenRepository repo, ISessionRepository sessionRepo)
        {
            _updateRepos = [];
            _addRepos = [];
            _getById = [];
            _getByToken = [];
            _getMultipleDependecies = [];
           
            _updateRepos[InstanceNamesEnum.RefreshToken] = 
                WrapUpdater<RefreshToken>(repo.UpdateAsync);
            
            _updateRepos[InstanceNamesEnum.UserSession] = 
                WrapUpdater<UserSession>(sessionRepo.UpdateUserSessionAsync);


            _addRepos[InstanceNamesEnum.RefreshToken] = 
                WrapAdder<RefreshToken>(repo.AddTokenAsync);
            
            _addRepos[InstanceNamesEnum.UserSession] = 
                WrapAdder<UserSession>(sessionRepo.AddUserSessionAsync);

            
            _getById[InstanceNamesEnum.UserSession] =
                WrapGetById(sessionRepo.GetUserSessionById);
            _getById[InstanceNamesEnum.RefreshToken] = WrapGetById(repo.GetTokenByIdAsync);
           
            _getByToken[InstanceNamesEnum.RefreshToken] =
                WrapGetByToken(repo.GetByRawTokenAsync);
           
            _getByToken[InstanceNamesEnum.UserSession] =
                WrapGetByToken(sessionRepo.GetByTokenAsync);


            
            _getMultipleDependecies[InstanceNamesEnum.RefreshToken] = 
                WrapGetMoreDependencyes(repo.GetTokensBySessionId);
            
            _getMultipleDependecies[InstanceNamesEnum.UserSession] =
                WrapGetMoreDependencyes(sessionRepo.GetActiveUserSessionAsync);
        }


        public async Task<bool> UpdateDependencyAsync<T>(InstanceNamesEnum key, T parameter) where T : IUserDependency
        {
            if (!_updateRepos.TryGetValue(key, out var factory)) throw new InvalidOperationException($"invalid {key}");

            return await factory(parameter);
        }
        public async Task AddDependencyAsync<T>(InstanceNamesEnum key, T parameter) where T : IUserDependency
        {
            if (!_addRepos.TryGetValue(key, out var factory)) throw new InvalidOperationException($"invalid {key}");
            await factory(parameter);
        }
        public async Task<T> GetDependencyByIdAsync<T>(InstanceNamesEnum key, Guid id) where T : IUserDependency
        {
            if (!_getById.TryGetValue(key, out var factory)) throw new InvalidOperationException($"invalid {key}");
            var result = await factory(id);


            return (T)result;
        }
        public async Task<T> GetDependencyByTokenAsync<T>(InstanceNamesEnum key, string rawToken) where T : IUserDependency
        {
            if (!_getByToken.TryGetValue(key, out var factory)) throw new InvalidOperationException($"invalid {key}");
            var result = await factory(rawToken);


            return (T)result;
        }

        public async Task<IEnumerable<T>> GetMultipleDependenciesAsync<T>(InstanceNamesEnum key, Guid specificId) where T : IUserDependency
        {
            if (!_getMultipleDependecies.TryGetValue(key, out var factory)) throw new InvalidOperationException($"invalid {key}");
            var result = await factory(specificId);

            return result.Cast<T>();
        }
    }
}
