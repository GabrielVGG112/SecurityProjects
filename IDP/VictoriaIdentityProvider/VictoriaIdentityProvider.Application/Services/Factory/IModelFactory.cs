using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.Factory
{
    public interface IModelFactory<T> where T : IUserDependency
    {
        (T ,string)CreateModel(User user);
    }
}
