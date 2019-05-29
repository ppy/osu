// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Beatmaps;

namespace osu.Game.Overlays.Profile.Sections
{
    public class UnderscoredBeatmapLink : UnderscoredLinkContainer
    {
        private readonly BeatmapInfo beatmap;

        public UnderscoredBeatmapLink(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapSetOverlay beatmapSetOverlay)
        {
            ClickAction = () =>
            {
                if (beatmap.OnlineBeatmapID != null)
                    beatmapSetOverlay?.FetchAndShowBeatmap(beatmap.OnlineBeatmapID.Value);
                else if (beatmap.BeatmapSet?.OnlineBeatmapSetID != null)
                    beatmapSetOverlay?.FetchAndShowBeatmapSet(beatmap.BeatmapSet.OnlineBeatmapSetID.Value);
            };
        }
    }
}
