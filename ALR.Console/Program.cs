using ALR.Common;
using ALR.Filebot;
using ALR.Files;
using ALR.qBittorrent;
using ALR.Utorrent;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ALR.Console
{
    class Program
    {
        public static IConfiguration Configuration { get; private set; }
        public static IServiceProvider ServiceProvider { get; private set; }

        static void Main( string[] args )
        {
            Log.Logger = new LoggerConfiguration()
               .WriteTo.File( "alr.log" )
               .CreateLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath( Directory.GetCurrentDirectory() )
                .AddJsonFile( "appsettings.json", optional: false, reloadOnChange: true )
                .AddJsonFile( "appsettings.Secrets.json", optional: true, reloadOnChange: true );

            Configuration = builder.Build();


            // Create a service collection and configure our depdencies
            var serviceCollection = new ServiceCollection();
            ConfigureServices( serviceCollection );

            // Build the our IServiceProvider and set our static reference to it
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var runner = ServiceProvider.GetRequiredService<Runner>();

            runner.RunAsync().GetAwaiter().GetResult();           
        }

        private static void ConfigureServices( IServiceCollection services )
        {
            // Make configuration settings available
            services.AddSingleton<IConfiguration>( Configuration );
            services.AddMediatR( typeof( FilebotHandler ) );
            services.AddMediatR( typeof( FileHandler ) );
            services.AddMediatR( typeof( UTorrentClientHandler ) );
            //services.AddMediatR( typeof( qBittorrentClientHandler ) );
            services.AddTransient( typeof(Runner) );
            services.AddLogging( configure => configure.AddSerilog() );
        }
    }
}
