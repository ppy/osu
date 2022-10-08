// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public enum ManiaScrollingDirection
    {
        [Description("从下往上")]
        Up = ScrollingDirection.Up,
        [Description("从上往下")]
        Down = ScrollingDirection.Down
    }
}
