using MediatR;
using System;
using System.Collections.Generic;

namespace ALR.Common
{
    public class DeleteTorrent : INotification
    {
        public TorrentDescriptor Torrent { get; set; }
    }

    public class MoveTorrent : INotification
    {
        public TorrentDescriptor Torrent { get; set; }
    }

    public class MoveToMediaLibrary : INotification { }

    public class SendEmailReport : INotification
    {
        public List<TorrentDescriptor> Torrents { get; set; }

        public SendEmailReport( List<TorrentDescriptor> torrents )
        {
            this.Torrents = torrents;
        }
    }
}
