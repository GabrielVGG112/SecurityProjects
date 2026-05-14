using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Domain.Interfaces
{
    public interface IUserDependency
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}