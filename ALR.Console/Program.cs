using ALR.Common;
using ALR.Filebot;
using ALR.Files;
using ALR.Utorrent;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AutomatedLivingRoom
{
    class Program
    {
        public static IConfiguration Configuration { get; private set; }
        public static IServiceProvider ServiceProvider { get; private set; }

        static void Main( string[] args )
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath( Directory.GetCurrentDirectory() )
                .AddJsonFile( "appsettings.json", optional: false, reloadOnChange: true );

            Configuration = builder.Build();


            // Create a service collection and configure our depdencies
            var serviceCollection = new ServiceCollection();
            ConfigureServices( serviceCollection );

            // Build the our IServiceProvider and set our static reference to it
            ServiceProvider = serviceCollection.BuildServiceProvider();

            IMediator mediator = ServiceProvider.GetRequiredService<IMediator>();

            Task.Run( async () =>
            {
                var torrents = await mediator.Send( new GetCompletedTorrents() );
            } ).Wait();
           
        }

        private static void ConfigureServices( IServiceCollection services )
        {
            // Make configuration settings available
            services.AddSingleton<IConfiguration>( Configuration );
            services.AddMediatR( typeof( FilebotHandler ) );
            services.AddMediatR( typeof( FileHandler ) );
            services.AddMediatR( typeof( UTorrentClientHandler ) );
        }
    }
}
