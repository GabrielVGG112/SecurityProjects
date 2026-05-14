using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.CustomErrors
{
    public class ValidationError : Exception
    {
        public IReadOnlyList<string> Errors { get;  }
        public ValidationError(IEnumerable<string>errors) : base ("Validation Failed")
        {
            Errors = errors.ToList();
        }

  
    } 
}
