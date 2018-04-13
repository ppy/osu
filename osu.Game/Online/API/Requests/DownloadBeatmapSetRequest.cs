// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using System;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : APIDownloadRequest
    {
        public readonly BeatmapSetInfo BeatmapSet;

        public Action<float> DownloadProgressed;

        private readonly bool noVideo;

        public DownloadBeatmapSetRequest(BeatmapSetInfo set, bool noVideo)
        {
            this.noVideo = noVideo;
            BeatmapSet = set;

            Progress += (current, total) => DownloadProgressed?.Invoke((float) current / total);
        }

        protected override string Target => $@"beatmapsets/{BeatmapSet.OnlineBeatmapSetID}/download{(noVideo ? "?noVideo=1" : "")}";
    }
}
