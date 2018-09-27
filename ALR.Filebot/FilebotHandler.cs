using Microsoft.Extensions.Configuration;
using System;
using MediatR;
using ALR.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ALR.Filebot
{
    public class FilebotHandler : INotificationHandler<MoveToMediaLibrary>
    {
        private readonly string m_filebotPath;
        private readonly string m_filebotCommandLine;
        private readonly string m_filebotOutput;
        private readonly string m_filebotInput;
        private readonly string m_filebotTvSeriesFormat;
        private readonly ILogger<FilebotHandler> m_logger;

        public FilebotHandler( IConfiguration configuration, ILogger<FilebotHandler> logger )
        {
            m_filebotPath = configuration[ "Filebot:Path" ];
            m_filebotCommandLine = configuration[ "Filebot:CommandLine" ];
            m_filebotOutput = configuration[ "Filebot:Output" ];
            m_filebotInput = configuration[ "Filebot:Input" ];
            m_filebotTvSeriesFormat = configuration[ "Filebot:Formats:TV" ];
            m_logger = logger;
        }

        public Task Handle( MoveToMediaLibrary notification, CancellationToken cancellationToken )
        {
            try
            {
                return MoveToMediaLibrary();
            }
            catch ( Exception ex )
            {
                m_logger.LogError( ex, "Couldnt move to media library" );
                throw;
            }
        }

        private Task MoveToMediaLibrary()
        {
            string args = m_filebotCommandLine
                            .Replace( "{output}", $"\"{m_filebotOutput}\"" )
                            .Replace( "{input}", $"\"{m_filebotInput}\"" )
                            .Replace( "{format}", $"\"{m_filebotTvSeriesFormat}\"" );

            Process p = new Process();
            ProcessStartInfo si = new ProcessStartInfo( m_filebotPath, args )
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            p.StartInfo = si;
            p.Start();
            while ( !p.StandardOutput.EndOfStream )
            {
                string line = p.StandardOutput.ReadLine();
                m_logger.LogInformation( line );
            }

            while ( !p.StandardError.EndOfStream )
            {
                string stderr = p.StandardError.ReadToEnd();
                if ( stderr != null )
                    m_logger.LogError( stderr );
            }

            p.WaitForExit();

            return Task.CompletedTask;
        }
    }
}
