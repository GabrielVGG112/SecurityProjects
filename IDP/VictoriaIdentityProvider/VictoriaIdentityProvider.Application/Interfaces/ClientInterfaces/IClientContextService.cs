using VictoriaIdentityProvider.Application.DTOs;

namespace VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces
{
    public interface IClientContextService
    {
        ClientMetadataDto GetClientMetadata();
    }
}