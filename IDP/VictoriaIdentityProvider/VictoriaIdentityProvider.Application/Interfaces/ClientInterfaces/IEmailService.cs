using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces
{
    public interface IEmailService
    {
        string CreateConfirmationLink(string token,string pathName);
        Task SendConfirmationEmail(string to, string subject, string body);
    }
}
