using ALR.Common;
using ALR.Console;
using ALR.Utorrent;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ALR.Tests
{
    public class EmailHandlerTests
    {
        [Fact( DisplayName = "Email handler can send emails" )]
        public async Task Get()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.SetBasePath( Directory.GetCurrentDirectory() );
            configurationBuilder.AddJsonFile( Directory.GetCurrentDirectory()  + @"\..\..\..\..\ALR.Console\appSettings.json", optional: false, reloadOnChange: true );
            configurationBuilder.AddJsonFile( Directory.GetCurrentDirectory()  + @"\..\..\..\..\ALR.Console\appsettings.Secrets.json", optional: false, reloadOnChange: true );

            var config = configurationBuilder.Build();

            var handler = new EmailHandler( config );
            await handler.Handle( new SendEmailReport( new List<TorrentDescriptor>() { new TorrentDescriptor() { Name = "Test!" } } ) , CancellationToken.None );
        }
    }
}
