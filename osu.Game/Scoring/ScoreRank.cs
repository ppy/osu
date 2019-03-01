// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Scoring
{
    public enum ScoreRank
    {
        [Description(@"F")]
        F,

        [Description(@"F")]
        D,

        [Description(@"C")]
        C,

        [Description(@"B")]
        B,

        [Description(@"A")]
        A,

        [Description(@"S")]
        S,

        [Description(@"SPlus")]
        SH,

        [Description(@"SS")]
        X,

        [Description(@"SSPlus")]
        XH,
    }
}
