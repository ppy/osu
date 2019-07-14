// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ClickAction = () =>
            {
                var beatmapId = beatmap.OnlineBeatmapID;

                if (beatmapId.HasValue)
                    Game?.ShowBeatmap(beatmapId.Value);
            };
        }
    }
}
