// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.IO.Network;
using osu.Game.Beatmaps;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : ArchiveDownloadRequest<IBeatmapSetInfo>
    {
        private readonly bool minimiseDownloadSize;

        public DownloadBeatmapSetRequest(IBeatmapSetInfo set, bool minimiseDownloadSize = false)
            : base(set)
        {
            this.minimiseDownloadSize = minimiseDownloadSize;
        }

        protected override string Target => $@"beatmapsets/{Model.OnlineID}/download{(minimiseDownloadSize ? "?noVideo=1" : "")}";

        protected override string Uri => $@"{API.WebsiteRootUrl}/api/v2/{Target}";

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Timeout = 60000;
            return req;
        }

        protected override string FileExtension => ".osz";
    }
}
