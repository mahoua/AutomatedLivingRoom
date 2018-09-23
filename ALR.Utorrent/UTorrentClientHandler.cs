using ALR.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UTorrent.Api;
using UTorrent.Api.Data;

namespace ALR.Utorrent
{
    public class UTorrentClientHandler : IRequestHandler<GetCompletedTorrents, List<TorrentDescriptor>>, INotificationHandler<DeleteTorrent>
    {
        private readonly UTorrentClient m_client;

        public UTorrentClientHandler( IConfiguration configuration )
        {
            var username = configuration[ "uTorrent:Username" ];
            var password = configuration[ "uTorrent:Password" ];
            var ip = configuration[ "uTorrent:IP" ];
            var port = configuration.GetValue<int>( "uTorrent:Port" );

            m_client = new UTorrentClient( ip, port, username, password );
        }

        public async Task<List<TorrentDescriptor>> Handle( GetCompletedTorrents request, CancellationToken cancellationToken )
        {
            var completed = new List<TorrentDescriptor>();

            var list = await m_client.GetListAsync();
            if ( list.Error != null )
            {
                throw list.Error;
            }
            var torrents = list.Result.Torrents;

            foreach ( var torrent in torrents )
            {
                if ( torrent.Progress == 30.0 )
                {
                    // Log( "Label " + torrent.Label.ToString() );

                    if ( torrent.Label == "TV" && request.Type == TorrentMediaType.TV )
                    {
                        completed.Add( ToDescriptor( torrent ) );
                    }
                    if ( torrent.Label.Contains( "Movie" ) && request.Type == TorrentMediaType.Movie )
                    {
                        completed.Add( ToDescriptor( torrent ) );
                    }
                }
            }

            return completed;
        }

        private static TorrentDescriptor ToDescriptor( Torrent torrent )
        {
            return new TorrentDescriptor()
            {
                Hash = torrent.Hash,
                SavedPath = torrent.Path
            };
        }

        public Task Handle( DeleteTorrent notification, CancellationToken cancellationToken )
        {
            return m_client.DeleteTorrentAsync( notification.Torrent.Hash );
        }
    }
}
