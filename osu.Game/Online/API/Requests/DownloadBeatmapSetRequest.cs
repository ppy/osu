// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using System;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : APIDownloadRequest
    {
        public readonly BeatmapSetInfo BeatmapSet;

        public float Progress;

        public event Action<float> DownloadProgressed;

        private readonly bool noVideo;

        public DownloadBeatmapSetRequest(BeatmapSetInfo set, bool noVideo)
        {
            this.noVideo = noVideo;
            BeatmapSet = set;

            Progressed += (current, total) => DownloadProgressed?.Invoke(Progress = (float)current / total);
        }

        protected override string Target => $@"beatmapsets/{BeatmapSet.OnlineBeatmapSetID}/download{(noVideo ? "?noVideo=1" : "")}";
    }
}
