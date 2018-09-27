using ALR.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using qBittorrentSharp;
using qBittorrentSharp.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ALR.qBittorrent
{
    public class qBittorrentClientHandler : IRequestHandler<GetCompletedTorrents, List<TorrentDescriptor>>, INotificationHandler<DeleteTorrent>
    {
        private readonly ILogger<qBittorrentClientHandler> m_logger;
        private readonly string m_username;
        private readonly string m_password;

        public qBittorrentClientHandler( IConfiguration configuration, ILogger<qBittorrentClientHandler> logger )
        {
            m_username = configuration[ "uTorrent:Username" ];
            m_password = configuration[ "uTorrent:Password" ];
            var ip = configuration[ "uTorrent:IP" ];
            var port = configuration.GetValue<int>( "uTorrent:Port" );

            API.Initialize( $"http://{ip}:{port}" );
            
            m_logger = logger;
        }

        public async Task<List<TorrentDescriptor>> Handle( GetCompletedTorrents request, CancellationToken cancellationToken )
        {
            await API.Login( m_username, m_password );

            m_logger.LogInformation( "Getting completed torrents" );

            var completed = new List<TorrentDescriptor>();

            try
            {
                var torrents = await API.GetTorrents();

                m_logger.LogInformation( "Found {count} torrents", torrents.Count );
                foreach ( var torrent in torrents )
                {
                    if ( torrent.Progress == 1000.0 )
                    {
                        // Log( "Label " + torrent.Label.ToString() );

                        if ( torrent.Category == "TV" && request.Type == TorrentMediaType.TV )
                        {
                            m_logger.LogInformation( "Torrent TV {name} is completed", torrent.Name );
                            completed.Add( ToDescriptor( torrent ) );
                        }
                        else if ( torrent.Category.Contains( "Movie" ) && request.Type == TorrentMediaType.Movie )
                        {
                            m_logger.LogInformation( "Torrent Movie {name} is completed", torrent.Name );
                            completed.Add( ToDescriptor( torrent ) );
                        }
                        else
                        {
                            m_logger.LogInformation( "Torrent {name} has label {label} which didnt match", torrent.Name, torrent.Category );
                        }
                    }
                }

            }
            catch ( Exception ex )
            {
                m_logger.LogError( ex, "Could not acquire list of torrents" );
                throw;
            }

            return completed;
        }

        public Task Handle( DeleteTorrent notification, CancellationToken cancellationToken )
        {
            m_logger.LogInformation( "Deleting torrent {torrent}", notification.Torrent.Name );
            return API.DeleteTorrents( new List<string>() { notification.Torrent.Hash } );
            // TODO do NOT delete permanently
        }

        private static TorrentDescriptor ToDescriptor( Torrent torrent )
        {
            return new TorrentDescriptor()
            {
                Hash = torrent.Hash,
                //SavePath = torrent.,
                Name = torrent.Name
            };
        }
    }
}
