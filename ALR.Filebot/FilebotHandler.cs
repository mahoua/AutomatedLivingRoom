using ALR.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                m_logger.LogDebug( "Calling FileBot" );
                MoveToMediaLibrary();
                m_logger.LogDebug( "FileBot terminated" );
            }
            catch ( Exception ex )
            {
                m_logger.LogError( ex, "Couldnt move to media library" );
            }
            return Task.CompletedTask;
        }

        private void MoveToMediaLibrary()
        {
            if ( IsDirectoryEmpty( m_filebotInput ) )
            {
                m_logger.LogInformation( "Directory is empty, no need to call filebot" );
                return;
            }

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
        }


        private static bool IsDirectoryEmpty( string path )
        {
            return !Directory.EnumerateFileSystemEntries( path ).Any();
        }
    }
}
