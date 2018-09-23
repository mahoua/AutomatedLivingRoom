using ALR.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace ALR.Console
{
    public class EmailHandler : INotificationHandler<SendEmailReport>
    {
        private readonly string m_from;
        private readonly string m_destination;
        private readonly string m_smtp;
        private readonly int m_port;
        private readonly bool m_enabled;
        private readonly string m_username;
        private readonly string m_password;

        public EmailHandler( IConfiguration configuration )
        {
            m_from = configuration[ "Email:From" ];
            m_destination = configuration[ "Email:Destination" ];
            m_smtp = configuration[ "Email:SMTP" ];
            m_port = configuration.GetValue<int>( "Email:Port" );
            m_enabled = configuration.GetValue<bool>( "Email:Enabled" );
            m_username = configuration[ "Email:Username" ];
            m_password = configuration[ "Email:Password" ];
        }

        public Task Handle( SendEmailReport notification, CancellationToken cancellationToken )
        {
            if ( m_enabled && notification.Torrents.Any() )
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress( m_from );
                mail.To.Add( m_destination );

                mail.Subject = string.Format( "[ALR] {0}", notification.Torrents.First().Name );
               
                mail.IsBodyHtml = true;
                mail.Body = string.Empty;

                // LiTEEM 1.0
                // Least Intelligent Templating Engine Ever Made 1.0
                mail.Body += "<html>";
                
                mail.Body += "</html>";
                SmtpClient smtp = new SmtpClient( m_smtp, m_port );
                smtp.Credentials = new NetworkCredential( m_username, m_password );
                smtp.EnableSsl = true;
                smtp.Send( mail );
            }

            return Task.CompletedTask;
        }
    }
}
