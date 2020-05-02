// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : ArchiveDownloadRequest<BeatmapSetInfo>
    {
        private readonly bool noVideo;
        private readonly bool IsMini;

        public DownloadBeatmapSetRequest(BeatmapSetInfo set, bool noVideo, bool IsMini = false)
            : base(set)
        {
            this.noVideo = noVideo;
            this.IsMini = IsMini;
        }

        private string CalcSayoUri()
        {
            int IdFull = (int)Model.OnlineBeatmapSetID;

            var Target = $@"{(IsMini? ( noVideo? "novideo" : "full") : "mini")}/{IdFull}";
            return Target;
        }

        protected override string Target => $@"{CalcSayoUri()}";

        protected override string Uri => $@"https://txy1.sayobot.cn/beatmaps/download/{Target}";
    }
}
