using ALR.Utorrent;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ALR.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
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

            var utorrent = new UTorrentClientHandler( config );
            var torrents = await utorrent.Handle(new Common.GetCompletedTorrents() { Type = Common.TorrentMediaType.TV }, CancellationToken.None );
        }
    }
}
