using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.CustomErrors
{
    public class ClaimException : Exception
    {
        public IReadOnlyList<string> Errors {  get; set; }
        public ClaimException(IReadOnlyList<string> exceptions) : base("Multiple Claim Exceptions Occured") 
        {
            Errors = exceptions;
        }

        public ClaimException(string? message) : base(message)
        {
            Errors = new List<string>();
        }

        public ClaimException(string? message, Exception? innerException) : base(message, innerException)
        {
            Errors = new List<string>();
        }
        
    }
}
