// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Scoring
{
    [LocalisableEnum(typeof(ScoreRankEnumLocalisationMapper))]
    public enum ScoreRank
    {
        [Description(@"D")]
        D,

        [Description(@"C")]
        C,

        [Description(@"B")]
        B,

        [Description(@"A")]
        A,

        [Description(@"S")]
        S,

        [Description(@"S+")]
        SH,

        [Description(@"SS")]
        X,

        [Description(@"SS+")]
        XH,
    }

    public class ScoreRankEnumLocalisationMapper : EnumLocalisationMapper<ScoreRank>
    {
        public override LocalisableString Map(ScoreRank value)
        {
            switch (value)
            {
                case ScoreRank.XH:
                    return BeatmapsStrings.RankXH;

                case ScoreRank.X:
                    return BeatmapsStrings.RankX;

                case ScoreRank.SH:
                    return BeatmapsStrings.RankSH;

                case ScoreRank.S:
                    return BeatmapsStrings.RankS;

                case ScoreRank.A:
                    return BeatmapsStrings.RankA;

                case ScoreRank.B:
                    return BeatmapsStrings.RankB;

                case ScoreRank.C:
                    return BeatmapsStrings.RankC;

                case ScoreRank.D:
                    return BeatmapsStrings.RankD;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
