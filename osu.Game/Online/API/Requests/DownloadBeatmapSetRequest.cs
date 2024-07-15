// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Beatmaps;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : ArchiveDownloadRequest<IBeatmapSetInfo>
    {
        private readonly bool noVideo;

        public DownloadBeatmapSetRequest(IBeatmapSetInfo set, bool noVideo)
            : base(set)
        {
            this.noVideo = noVideo;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Timeout = 60000;
            return req;
        }

        protected override string FileExtension => ".osz";

        protected override string Target => $@"beatmapsets/{Model.OnlineID}/download{(noVideo ? "?noVideo=1" : "")}";
    }
}
