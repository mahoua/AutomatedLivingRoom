﻿using ALR.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UTorrentClientHandler> m_logger;

        public UTorrentClientHandler( IConfiguration configuration, ILogger<UTorrentClientHandler> logger )
        {
            var username = configuration[ "uTorrent:Username" ];
            var password = configuration[ "uTorrent:Password" ];
            var ip = configuration[ "uTorrent:IP" ];
            var port = configuration.GetValue<int>( "uTorrent:Port" );

            m_client = new UTorrentClient( ip, port, username, password );
            m_logger = logger;
        }

        public async Task<List<TorrentDescriptor>> Handle( GetCompletedTorrents request, CancellationToken cancellationToken )
        {
            m_logger.LogDebug( "Getting completed torrents" );
            try
            {
                return await GetCompletedTorrents( request );
            }
            catch ( System.Exception ex )
            {
                m_logger.LogError( ex, "Couldnt get completed torrents" );
                throw;
            }
        }

        private async Task<List<TorrentDescriptor>> GetCompletedTorrents( GetCompletedTorrents request )
        {
            var completed = new List<TorrentDescriptor>();

            var list = await m_client.GetListAsync();
            if ( list.Error != null )
            {
                m_logger.LogError( list.Error, "Could not acquire list of torrents" );
                throw list.Error;
            }
            var torrents = list.Result.Torrents;

            m_logger.LogInformation( "Found {count} torrents", torrents.Count );
            foreach ( var torrent in torrents )
            {
                if ( torrent.Progress == 1000.0 )
                {
                    // Log( "Label " + torrent.Label.ToString() );

                    if ( torrent.Label == "TV" && request.Type == TorrentMediaType.TV )
                    {
                        m_logger.LogInformation( "Torrent TV {name} is completed", torrent.Name );
                        completed.Add( ToDescriptor( torrent ) );
                    }
                    else if ( torrent.Label.Contains( "Movie" ) && request.Type == TorrentMediaType.Movie )
                    {
                        m_logger.LogInformation( "Torrent Movie {name} is completed", torrent.Name );
                        completed.Add( ToDescriptor( torrent ) );
                    }
                    else
                    {
                        m_logger.LogInformation( "Torrent {name} has label {label} which didnt match", torrent.Name, torrent.Label );
                    }
                }
            }

            return completed;
        }

        public Task Handle( DeleteTorrent notification, CancellationToken cancellationToken )
        {
            m_logger.LogInformation( "Deleting torrent {torrent}", notification.Torrent.Name );
            return m_client.DeleteTorrentAsync( notification.Torrent.Hash );
        }

        private static TorrentDescriptor ToDescriptor( Torrent torrent )
        {
            return new TorrentDescriptor()
            {
                Hash = torrent.Hash,
                SavePath = torrent.Path,
                Name = torrent.Name
            };
        }
    }
}
