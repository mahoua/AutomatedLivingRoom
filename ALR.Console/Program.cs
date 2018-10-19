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
using System.Reflection;
using System.Threading.Tasks;

namespace ALR.Console
{
    class Program
    {
        public static IConfiguration Configuration { get; private set; }
        public static IServiceProvider ServiceProvider { get; private set; }

        static void Main( string[] args )
        {
            var currentFolder = Path.GetDirectoryName( Assembly.GetEntryAssembly().Location );

            Log.Logger = new LoggerConfiguration()
               .WriteTo.File( Path.Combine( currentFolder, "alr.log" ) )
               .WriteTo.Console()
               .CreateLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath( currentFolder )
                .AddJsonFile( "appsettings.json", optional: false, reloadOnChange: true )
                .AddJsonFile( "appsettings.Secrets.json", optional: true, reloadOnChange: true );

            Configuration = builder.Build();

            // Create a service collection and configure our depdencies
            var serviceCollection = new ServiceCollection();
            ConfigureServices( serviceCollection );

            // Build the our IServiceProvider and set our static reference to it
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var runner = ServiceProvider.GetRequiredService<Runner>();

            try
            {
                runner.RunAsync().GetAwaiter().GetResult();
            }
            catch ( Exception ex )
            {
                Log.Logger.Error( ex, "Error in program.cs" );
            }
        }

        private static void ConfigureServices( IServiceCollection services )
        {
            // Make configuration settings available
            services.AddSingleton( Configuration );
            services.AddMediatR( typeof( EmailHandler ), typeof( FilebotHandler ), typeof( FileHandler ), typeof( UTorrentClientHandler ) );
            //services.AddMediatR( typeof( qBittorrentClientHandler ) );
            services.AddTransient( typeof(Runner) );
            services.AddLogging( configure => configure.AddSerilog() );
        }
    }
}
