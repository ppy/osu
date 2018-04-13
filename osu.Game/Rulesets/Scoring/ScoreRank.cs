// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Rulesets.Scoring
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
