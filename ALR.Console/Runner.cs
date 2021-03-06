﻿using ALR.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ALR.Console
{
    public class Runner
    {
        private readonly IMediator m_mediator;
        private readonly ILogger<Runner> m_logger;

        public Runner( IMediator mediator, ILogger<Runner> logger )
        {
            m_mediator = mediator;
            m_logger = logger;
        }

        public async Task RunAsync()
        {
            try
            {
                var torrents = await m_mediator.Send( new GetCompletedTorrents() { Type = TorrentMediaType.TV } );

                foreach ( var torrent in torrents )
                {
                    await m_mediator.Publish( new DeleteTorrent() { Torrent = torrent } );
                    await m_mediator.Publish( new MoveTorrent() { Torrent = torrent } );
                }

                await m_mediator.Publish( new MoveToMediaLibrary() );
                
                await m_mediator.Publish( new SendEmailReport( torrents ) );
            }
            catch ( System.Exception ex )
            {
                m_logger.LogError( ex, "Error in Runner" );
                throw;
            }
        }
    }

}
