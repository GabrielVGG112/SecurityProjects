using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Domain.CustomErrors;

namespace VictoriaIdentityProvider.Application.Services.ClientServices
{
    public class EmailService :IEmailService
    {
        private readonly SmtpOptions _options;
  

 
        public EmailService(IOptions<SmtpOptions> options )  
        {
            _options = options.Value;
       
        }
        public string CreateConfirmationLink(string token, string pathName) 
            => $"{_options.BaseUrl}/api/Registration/{pathName}?token={token}";
       
        public async Task SendConfirmationEmail(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_options.Host);
                client.Port = _options.Port;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_options.User, _options.Pass);

                var mail = new MailMessage
                {
                    From = new MailAddress(_options.User, "Victoria Identity Provider"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true

                };
                mail.To.Add(to);

                await client.SendMailAsync(mail);
            }
            
            catch (SmtpException)
           
            {
                throw new EmailDeliveryException(
                         $"Failed to send email to {to}");
            }
            
            catch (FormatException)
           
            {
                throw new EmailDeliveryException(
                         $"Invalid email address format: {to}");
            }
            
            catch (Exception)

            {
                throw new EmailDeliveryException(
                             $"Unexpected error sending email to {to}");
            }
        }

    }
}

