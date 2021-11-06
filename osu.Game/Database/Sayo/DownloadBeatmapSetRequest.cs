// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Beatmaps;
using osu.Game.Online.API;

namespace osu.Game.Database.Sayo
{
    public class SayoDownloadBeatmapSetRequest : ArchiveDownloadRequest<BeatmapSetInfo>
    {
        private readonly bool noVideo;
        private readonly bool mini;

        public SayoDownloadBeatmapSetRequest(BeatmapSetInfo set, bool noVideo, bool mini)
            : base(set)
        {
            this.noVideo = noVideo;
            this.mini = mini;
        }

        private string getTarget()
        {
            string idFull = Model.OnlineBeatmapSetID.ToString();

            string target = $@"{(mini ? "mini" : (noVideo ? "novideo" : "full"))}/{idFull}";
            return target;
        }

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
