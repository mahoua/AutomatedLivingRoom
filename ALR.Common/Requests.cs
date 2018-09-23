using MediatR;
using System;
using System.Collections.Generic;

namespace ALR.Common
{
    public enum TorrentMediaType
    {
        TV,
        Movie
    }

    public class GetCompletedTorrents : IRequest<List<TorrentDescriptor>>
    {
        public TorrentMediaType Type { get; set; }
    }
}
