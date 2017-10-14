// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using System;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : APIDownloadRequest
    {
        public readonly BeatmapSetInfo BeatmapSet;

        public Action<float> DownloadProgressed;

        public DownloadBeatmapSetRequest(BeatmapSetInfo set)
        {
            BeatmapSet = set;

            Progress += (current, total) => DownloadProgressed?.Invoke((float) current / total);
        }

        protected override string Target => $@"beatmapsets/{BeatmapSet.OnlineBeatmapSetID}/download";
    }
}
