// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapListingSortTabControl : OverlaySortTabControl<BeatmapSortCriteria>
    {
        public BeatmapListingSortTabControl()
        {
            Current.Value = BeatmapSortCriteria.Ranked;
        }
    }

    public enum BeatmapSortCriteria
    {
        Title,
        Artist,
        Difficulty,
        Ranked,
        Rating,
        Plays,
        Favourites,
    }
}
