using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces
{

    public interface ILoginService
    {
        Task<User> LoginUserAsync(LoginDto dto);
    }
}
