using MediatR;
using System;

namespace ALR.Common
{
    public class DeleteTorrent : INotification
    {
        public TorrentDescriptor Torrent { get; set; }
    }
}
