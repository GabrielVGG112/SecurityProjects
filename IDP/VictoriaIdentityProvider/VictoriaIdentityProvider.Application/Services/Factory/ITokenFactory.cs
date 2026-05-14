using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.Factory
{
    public interface ITokenFactory : IModelFactory<RefreshToken>
    {
        string GenerateJwtToken(User user, Guid sessionId);
    }
}
