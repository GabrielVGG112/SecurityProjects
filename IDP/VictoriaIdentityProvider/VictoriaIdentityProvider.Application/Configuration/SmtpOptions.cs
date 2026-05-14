using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Application.Configuration
{
    /// <summary>
    /// Represents configuration options for connecting to an SMTP server.
    /// </summary>
    /// <remarks>Use this class to specify the connection details required for sending email via SMTP,
    /// including the server host, port, and authentication credentials. These options are typically used to configure
    /// email clients or services that send messages through an SMTP server.</remarks>
    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;    
        public int Port { get; set; }
        public string User { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;

    }
}
