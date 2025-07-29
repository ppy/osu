// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Screens.Select.Leaderboards
{
    public enum LeaderboardSortMode
    {
        [LocalisableDescription(typeof(BeatmapLeaderboardWedgeStrings), nameof(BeatmapLeaderboardWedgeStrings.Score))]
        Score,

        [LocalisableDescription(typeof(BeatmapLeaderboardWedgeStrings), nameof(BeatmapLeaderboardWedgeStrings.Accuracy))]
        Accuracy,

        [LocalisableDescription(typeof(BeatmapLeaderboardWedgeStrings), nameof(BeatmapLeaderboardWedgeStrings.MaxCombo))]
        MaxCombo,

        [LocalisableDescription(typeof(BeatmapLeaderboardWedgeStrings), nameof(BeatmapLeaderboardWedgeStrings.Misses))]
        Misses,

        [LocalisableDescription(typeof(BeatmapLeaderboardWedgeStrings), nameof(BeatmapLeaderboardWedgeStrings.Date))]
        Date,
    }
}
