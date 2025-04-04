// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.Select.Filter
{
    public enum ScoreSortMode
    {
        [Description("Total score")]
        TotalScore,

        [Description("Accuracy")]
        Accuracy,

        [Description("Combo")]
        Combo,

        [Description("Miss count")]
        MissCount,

        [Description("Date")]
        Date,
    }
}
