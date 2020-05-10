// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : ArchiveDownloadRequest<BeatmapSetInfo>
    {
        private readonly bool noVideo;
        private readonly bool IsMini;

        private bool UseSayobot;

        public DownloadBeatmapSetRequest(BeatmapSetInfo set, bool UseSayobot, bool noVideo, bool IsMini = false)
            : base(set)
        {
            this.noVideo = noVideo;
            this.IsMini = IsMini;
            this.UseSayobot = UseSayobot;
        }

        private string CalcTarget()
        {
            switch ( UseSayobot )
            {
                case true:
                    var IdFull = Model.OnlineBeatmapSetID.ToString();

                    var Target = $@"{(IsMini? "mini" : ( noVideo? "novideo" : "full"))}/{IdFull}";
                    return Target;
                case false:
                default:
                    return $@"beatmapsets/{Model.OnlineBeatmapSetID}/download{(noVideo ? "?noVideo=1" : "")}";
            }
        }

        private string CalcUri()
        {
            switch ( UseSayobot )
            {
                case true:
                    return $@"https://txy1.sayobot.cn/beatmaps/download/{Target}";

                case false:
                default:
                    return $@"{API.Endpoint}/api/v2/{Target}";
            }
        }
    
        protected override string Target => $@"{CalcTarget()}";

        protected override string Uri => $@"{CalcUri()}";
    }
}
