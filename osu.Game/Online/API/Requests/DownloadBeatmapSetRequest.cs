// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Beatmaps;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : ArchiveDownloadRequest<BeatmapSetInfo>
    {
        private readonly bool noVideo;
        private readonly bool isMini;

        private readonly bool useSayobot;

        public DownloadBeatmapSetRequest(BeatmapSetInfo set, bool useSayobot, bool noVideo, bool isMini = false)
            : base(set)
        {
            this.noVideo = noVideo;
            this.isMini = isMini;
            this.useSayobot = useSayobot;
        }

        private string getTarget()
        {
            if (useSayobot)
            {
                var idFull = Model.OnlineBeatmapSetID.ToString();

                var target = $@"{(isMini ? "mini" : (noVideo ? "novideo" : "full"))}/{idFull}";
                return target;
            }

            return $@"beatmapsets/{Model.OnlineBeatmapSetID}/download{(noVideo ? "?noVideo=1" : "")}";
        }

        private string selectUri()
        {
            if (useSayobot)
                return $@"https://txy1.sayobot.cn/beatmaps/download/{Target}";

            return $@"{API.WebsiteRootUrl}/api/v2/{Target}";
        }

        protected override string Target => $@"{getTarget()}";

        protected override string Uri => $@"{selectUri()}";

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Timeout = 60000;
            return req;
        }

        protected override string FileExtension => ".osz";
    }
}
