// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : ArchiveDownloadRequest<BeatmapSetInfo>
    {
        private readonly bool noVideo;

        public DownloadBeatmapSetRequest(BeatmapSetInfo set, bool noVideo)
            : base(set)
        {
            this.noVideo = noVideo;
        }

        private string CalcSayoUri()
        {
            int IdFull = (int)Model.OnlineBeatmapSetID;
            int IdHead = (int)Math.Floor(IdFull / 10000f);
            string IdTail = ( IdFull - (IdHead * 10000) ).ToString();

            if ( IdTail.ToString().Length != 4 )
            {
                for (int i = IdTail.ToString().Length; i < 4; i++)
                {
                    IdTail = "0" + $"{IdTail}";
                }
            }

            var Target = $@"{IdHead}/{IdTail}/{( noVideo? "novideo" : "full")}?filename=b-{IdFull}";
            return Target;
        }

        private string UpdateUriRoot()
        {
            var uri = $"https://b2.sayobot.cn:25225/beatmaps/{Target}";
            return uri;
        }

        protected override string Target => $@"{CalcSayoUri()}";

        protected override string Uri => $@"{UpdateUriRoot()}";
    }
}
