// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Screens.Select.Leaderboards
{
    public enum BeatmapLeaderboardScope
    {
        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Local))]
        Local,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Global))]
        Global,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Country))]
        Country,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Friend))]
        Friend,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Team))]
        Team,
    }
}
