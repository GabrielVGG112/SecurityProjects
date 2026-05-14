using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.CustomErrors
{
    public class EmailDeliveryException : Exception
    {
        public EmailDeliveryException()
        {
        }

        public EmailDeliveryException(string? message) : base(message)
        {
        }

        public EmailDeliveryException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
