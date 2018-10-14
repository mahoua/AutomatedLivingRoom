using ALR.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ALR.Files
{
    public class FileHandler : INotificationHandler<MoveTorrent>
    {
        private readonly string m_moveDestination;
        private readonly ILogger<FileHandler> m_logger;

        public FileHandler( IConfiguration configuration, ILogger<FileHandler> logger )
        {
            m_moveDestination = configuration[ "MoveDestination:TV" ];
            m_logger = logger;
            try
            {
                Directory.CreateDirectory( m_moveDestination );
            }
            catch ( System.Exception ex )
            {
                m_logger.LogError( ex, "Couldn't create destination folder" );
            }
        }

        public Task Handle( MoveTorrent notification, CancellationToken cancellationToken )
        {
            var folder = Path.GetFileName( notification.Torrent.SavePath );
            var destination = Path.Combine( m_moveDestination, folder );
            m_logger.LogInformation( "Moving {torrent} to {destination}", notification.Torrent.Name, destination );
            Directory.Move( notification.Torrent.SavePath, destination );
            return Task.CompletedTask;
        }
    }
}
