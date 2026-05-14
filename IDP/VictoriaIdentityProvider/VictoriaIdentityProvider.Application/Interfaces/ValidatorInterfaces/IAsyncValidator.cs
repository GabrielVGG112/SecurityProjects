using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Application.Services.Factory;

namespace VictoriaIdentityProvider.Application.Interfaces.ValidatorInterfaces
{
    public   interface IAsyncValidator<in T>
    {
        Task<ValidationResult> Validate(T input);
    }
}
