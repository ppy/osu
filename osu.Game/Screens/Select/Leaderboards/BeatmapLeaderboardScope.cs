// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Screens.Select.Leaderboards
{
    public enum BeatmapLeaderboardScope
    {
        [LocalisableDescription(typeof(BeatmapLeaderboardWedgeStrings), nameof(BeatmapLeaderboardWedgeStrings.Local))]
        Local,

        [LocalisableDescription(typeof(BeatmapLeaderboardWedgeStrings), nameof(BeatmapLeaderboardWedgeStrings.Global))]
        Global,

        [LocalisableDescription(typeof(BeatmapLeaderboardWedgeStrings), nameof(BeatmapLeaderboardWedgeStrings.Country))]
        Country,

        [LocalisableDescription(typeof(BeatmapLeaderboardWedgeStrings), nameof(BeatmapLeaderboardWedgeStrings.Friend))]
        Friend,

        [LocalisableDescription(typeof(BeatmapLeaderboardWedgeStrings), nameof(BeatmapLeaderboardWedgeStrings.Team))]
        Team,
    }
}
