using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.CustomErrors
{
    public class EmailNotVerifiedException : Exception
    {
        public EmailNotVerifiedException()
        {
        }

        public EmailNotVerifiedException(string? message) : base(message)
        {
        }

        public EmailNotVerifiedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
