using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Application.Services.Factory;

namespace VictoriaIdentityProvider.Application.Interfaces.ValidatorInterfaces
{
    /// <summary>
    /// Defines a mechanism for validating objects of a specified type and returning the results of the validation.
    /// </summary>
    /// <remarks>Implementations of this interface provide validation logic for objects of type T. The
    /// interface is typically used to encapsulate validation rules and to separate validation concerns from business
    /// logic.</remarks>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    public interface IValidator<in T>
    {
        ValidationResult Validate(T input);
    }
}
