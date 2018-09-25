using ALR.Common;
using ALR.Utorrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ALR.Tests
{
    public class TorrentClientHandlerTests
    {
        [Fact( DisplayName = "Utorrent client handler can get completed torrents" )]
        public async Task Get()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.SetBasePath( Directory.GetCurrentDirectory() );
            configurationBuilder.AddJsonFile( Directory.GetCurrentDirectory() + @"\..\..\..\..\ALR.Console\appSettings.json", optional: false, reloadOnChange: true );
            configurationBuilder.AddJsonFile( Directory.GetCurrentDirectory() + @"\..\..\..\..\ALR.Console\appsettings.Secrets.json", optional: false, reloadOnChange: true );
            
            var config = configurationBuilder.Build();

            var handler = new UTorrentClientHandler( config, new Logger<UTorrentClientHandler>( new NullLoggerFactory() ) );
            var torrents = await handler.Handle( new GetCompletedTorrents() { Type = TorrentMediaType.TV }, CancellationToken.None );
        }

        [Fact( DisplayName = "Utorrent client handler can delete torrents" )]
        public async Task Delete()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.SetBasePath( Directory.GetCurrentDirectory() );
            configurationBuilder.AddJsonFile( Directory.GetCurrentDirectory() + @"\..\..\..\..\ALR.Console\appSettings.json", optional: false, reloadOnChange: true );
            configurationBuilder.AddJsonFile( Directory.GetCurrentDirectory() + @"\..\..\..\..\ALR.Console\appsettings.Secrets.json", optional: false, reloadOnChange: true );

            var config = configurationBuilder.Build();

            var handler = new UTorrentClientHandler( config, new Logger<UTorrentClientHandler>( new NullLoggerFactory() ) );
            var torrents = await handler.Handle( new GetCompletedTorrents() { Type = TorrentMediaType.TV }, CancellationToken.None );
            foreach ( var torrent in torrents )
            {
                await handler.Handle( new DeleteTorrent() { Torrent = torrent }, CancellationToken.None );
            }
        }
    }
}
