using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.CustomErrors
{
    public class LockoutExpirationException : Exception
    {
        public LockoutExpirationException()
        {
        }

        public LockoutExpirationException(string? message) : base(message)
        {
        }

        public LockoutExpirationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
