// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Scoring
{
    public enum ScoreRank
    {
        // TODO: Localisable?
        [Description(@"F")]
        F = -1,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankD))]
        [Description(@"D")]
        D,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankC))]
        [Description(@"C")]
        C,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankB))]
        [Description(@"B")]
        B,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankA))]
        [Description(@"A")]
        A,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankS))]
        [Description(@"S")]
        S,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankSH))]
        [Description(@"S+")]
        SH,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankX))]
        [Description(@"SS")]
        X,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.RankXH))]
        [Description(@"SS+")]
        XH,
    }
}
