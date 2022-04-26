// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;

namespace osu.Game.Beatmaps.Timing
{
    [Obsolete("Use osu.Game.Beatmaps.Timing.TimeSignature instead.")]
    public enum TimeSignatures // can be removed 20220722
    {
        [Description("4/4")]
        SimpleQuadruple = 4,

        [Description("3/4")]
        SimpleTriple = 3
    }
}
