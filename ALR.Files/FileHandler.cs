using ALR.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ALR.Files
{
    public class FileHandler : INotificationHandler<MoveTorrent>
    {
        private string m_moveDestination;

        public FileHandler( IConfiguration configuration )
        {
            m_moveDestination = configuration[ "MoveDestination:TV" ];
            Directory.CreateDirectory( m_moveDestination );
        }

        public Task Handle( MoveTorrent notification, CancellationToken cancellationToken )
        {
            Directory.Move( notification.Torrent.SavePath, m_moveDestination );
            return Task.CompletedTask;
        }
    }
}
