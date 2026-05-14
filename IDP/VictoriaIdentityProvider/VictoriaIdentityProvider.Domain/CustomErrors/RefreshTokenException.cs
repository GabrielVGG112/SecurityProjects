using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.CustomErrors
{
    public class RefreshTokenException : Exception

    {
      public   IReadOnlyList <string> Errors { get; set; }
        public RefreshTokenException(List<string> errors) : base("Validation Failed")
        {
            Errors = errors;
        }

      

    }
    public class RefreshTokenConcurencyException :Exception
    {
        public IReadOnlyList<string> Errors { get; set; }
        public RefreshTokenConcurencyException(List<string> errors) : base("Concurrency Validation Failed")
        {
            Errors = errors;
        }
    }
}
