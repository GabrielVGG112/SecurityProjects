using Microsoft.AspNetCore.Http;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;

namespace VictoriaIdentityProvider.Application.Services.ClientServices;

public class ClientContextService : IClientContextService
{
    private readonly IHttpContextAccessor _accesor;

    public ClientContextService(IHttpContextAccessor accesor)
    {
        _accesor = accesor;
    }
    public ClientMetadataDto GetClientMetadata()
    {
        var ctx = _accesor.HttpContext;
        string forwarded = TryToGetTheValue("X-Forwarded-For");

        string ip = forwarded
            .Equals("unknown")
            ? ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown"
            : forwarded;

        return new ClientMetadataDto
        {
            IpAddress = ip,
            UserAgent = TryToGetTheValue("User-Agent"),
            DeviceId = TryToGetTheValue("X-Device-Id"),
            DeviceName = TryToGetTheValue("X-Device-Name")
        };
    }


    private string TryToGetTheValue(string key)
    {
        var value = _accesor.HttpContext?
                    .Request?
                    .Headers[key]
                    .FirstOrDefault();

        return string
            .IsNullOrWhiteSpace(value) 
            ? "unknown" 
            : value;
    }
}
