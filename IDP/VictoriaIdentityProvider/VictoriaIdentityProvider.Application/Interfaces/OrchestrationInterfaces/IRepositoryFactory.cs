using VictoriaIdentityProvider.Application.Enums;
using VictoriaIdentityProvider.Domain.Interfaces;

namespace VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces
{
    public interface IRepositoryFactory
    {
        Task AddDependencyAsync<T>(InstanceNamesEnum key, T parameter) where T : IUserDependency;
        Task<T> GetDependencyByIdAsync<T>(InstanceNamesEnum key, Guid id) where T : IUserDependency;
        Task<T> GetDependencyByTokenAsync<T>(InstanceNamesEnum key, string rawToken) where T : IUserDependency;
        Task<bool> UpdateDependencyAsync<T>(InstanceNamesEnum key, T parameter) where T : IUserDependency;
        Task<IEnumerable<T>> GetMultipleDependenciesAsync<T>(InstanceNamesEnum key, Guid specificId) where T : IUserDependency;
    }
}