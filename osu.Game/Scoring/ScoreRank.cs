// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Scoring
{
    public enum ScoreRank
    {
        // TODO: Localisable?
        F = -1,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankD))]
        D,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankC))]
        C,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankB))]
        B,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankA))]
        A,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankS))]
        S,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankSH))]
        // ReSharper disable once InconsistentNaming
        SH,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankX))]
        X,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankXH))]
        // ReSharper disable once InconsistentNaming
        XH,
    }
}
