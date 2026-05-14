using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.CustomErrors
{
    public class SessionConcurrencyException : Exception
    {
        public SessionConcurrencyException()
        {
        }

        public SessionConcurrencyException(string? message) : base(message)
        {
        }

        public SessionConcurrencyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
