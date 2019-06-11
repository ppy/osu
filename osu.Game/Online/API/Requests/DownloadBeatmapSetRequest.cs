// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;

namespace osu.Game.Online.API.Requests
{
    public class DownloadBeatmapSetRequest : ArchiveDownloadModelRequest<BeatmapSetInfo>
    {
        private readonly bool noVideo;
        private readonly BeatmapSetInfo set;

        public DownloadBeatmapSetRequest(BeatmapSetInfo set, bool noVideo)
            : base(set)
        {
            this.noVideo = noVideo;
            this.set = set;
        }

        protected override string Target => $@"beatmapsets/{set.OnlineBeatmapSetID}/download{(noVideo ? "?noVideo=1" : "")}";
    }
}
