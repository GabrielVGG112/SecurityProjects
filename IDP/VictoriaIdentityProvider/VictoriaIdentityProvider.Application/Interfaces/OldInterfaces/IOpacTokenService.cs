using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Application.DTOs.oldDtos;

namespace VictoriaIdentityProvider.Application.Interfaces.OldInterfaces
{
    [Obsolete]
    public interface IOpacTokenService
    {
        BundleTokenDto GenerateToken();
    }
}
