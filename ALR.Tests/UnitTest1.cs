using ALR.Common;
using ALR.Utorrent;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ALR.Tests
{
    public class TorrentClientHandler
    {
        [Fact( DisplayName = "Utorrent client handler can get completed torrents" )]
        public async Task Get()
        {
            Dictionary<string, string> arrayDict = new Dictionary<string, string>
            {
                {"uTorrent:Username", "admin"},
                {"uTorrent:Password", ""},
                {"uTorrent:IP", "192.168.0.103"},
                {"uTorrent:Port", "8080"}
            };

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection( arrayDict );
            var config = configurationBuilder.Build();

            var handler = new UTorrentClientHandler( config );
            var torrents = await handler.Handle( new GetCompletedTorrents() { Type = TorrentMediaType.TV }, CancellationToken.None );
        }

        [Fact( DisplayName = "Utorrent client handler can delete torrents" )]
        public async Task Delete()
        {
            Dictionary<string, string> arrayDict = new Dictionary<string, string>
            {
                {"uTorrent:Username", "admin"},
                {"uTorrent:Password", ""},
                {"uTorrent:IP", "192.168.0.103"},
                {"uTorrent:Port", "8080"}
            };

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection( arrayDict );
            var config = configurationBuilder.Build();

            var handler = new UTorrentClientHandler( config );
            var torrents = await handler.Handle( new Common.GetCompletedTorrents() { Type = TorrentMediaType.TV }, CancellationToken.None );
            foreach ( var torrent in torrents )
            {
                await handler.Handle( new DeleteTorrent() { Torrent = torrent }, CancellationToken.None );
            }
        }
    }
}
