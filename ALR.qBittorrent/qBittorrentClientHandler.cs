using MediatR;
using System;
using System.Collections.Generic;
using ALR.Common;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using qBittorrent.qBittorrentApi;

namespace ALR.qBittorrent
{
    public class qBittorrentClientHandler : IRequestHandler<GetCompletedTorrents, List<TorrentDescriptor>>, INotificationHandler<DeleteTorrent>
    {
        private readonly Api m_client;
        private readonly ILogger<qBittorrentClientHandler> m_logger;

        public qBittorrentClientHandler( IConfiguration configuration, ILogger<qBittorrentClientHandler> logger )
        {
            var username = configuration[ "uTorrent:Username" ];
            var password = configuration[ "uTorrent:Password" ];
            var ip = configuration[ "uTorrent:IP" ];
            var port = configuration.GetValue<int>( "uTorrent:Port" );
            
            m_client = new Api( new ServerCredential( new Uri($"http://{ip}:{port}"), username, password ) );
            m_logger = logger;
        }

        public async Task<List<TorrentDescriptor>> Handle( GetCompletedTorrents request, CancellationToken cancellationToken )
        {
            m_logger.LogInformation( "Getting completed torrents" );

            var completed = new List<TorrentDescriptor>();

            try
            {
                var torrents = await m_client.GetTorrents();

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
            return m_client.DeletePermanently( new List<string>() { notification.Torrent.Hash } );
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
