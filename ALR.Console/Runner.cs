using ALR.Common;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ALR.Console
{
    public class Runner
    {
        private readonly IMediator m_mediator;

        public Runner( IMediator mediator )
        {
            m_mediator = mediator;
        }

        public async Task RunAsync()
        {
            var torrents = await m_mediator.Send( new GetCompletedTorrents() { Type = TorrentMediaType.TV } );

            foreach ( var torrent in torrents )
            {
                await m_mediator.Publish( new DeleteTorrent() { Torrent = torrent } );
                await m_mediator.Publish( new MoveTorrent() { Torrent = torrent } );
            }

            await m_mediator.Publish( new MoveToMediaLibrary() );

            await m_mediator.Publish( new SendEmailReport( torrents ) );
            // todo log errors
        }
    }

}
