using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.CustomErrors
{
    public class SessionException : Exception
    {
        public IReadOnlyList<string> Errors { get; set; }
        public SessionException(List<string> errors) : base("Validation Failed")
        {
            Errors= errors;
        }

        public SessionException(string? message) : base(message)
        {
            Errors = new List<string>();
            
        }

        public SessionException(string? message, Exception? innerException) : base(message, innerException)
        {
            Errors = new List<string>();
        }
    }
}
