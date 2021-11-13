// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Beatmaps;
using osu.Game.Online.API;

namespace osu.Game.Database
{
    public class SayoDownloadBeatmapSetRequest : ArchiveDownloadRequest<IBeatmapSetInfo>
    {
        private readonly bool minimiseDownloadSize;

        public SayoDownloadBeatmapSetRequest(IBeatmapSetInfo set, bool minimiseDownloadSize)
            : base(set)
        {
            this.minimiseDownloadSize = minimiseDownloadSize;
        }

        private string getTarget() => $@"{(minimiseDownloadSize ? "novideo" : "full")}/{Model.OnlineID}";

        private string selectUri() => $@"https://txy1.sayobot.cn/beatmaps/download/{Target}";

        protected override string Target => getTarget();

        protected override string Uri => selectUri();

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Timeout = 60000;
            return req;
        }

        protected override string FileExtension => ".osz";
    }
}
