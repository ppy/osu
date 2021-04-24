// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Beatmaps.Timing
{
    public enum TimeSignatures
    {
        [Description("四分之一拍(4/4)")]
        SimpleQuadruple = 4,

        [Description("三分之一拍(3/4)")]
        SimpleTriple = 3
    }
}
