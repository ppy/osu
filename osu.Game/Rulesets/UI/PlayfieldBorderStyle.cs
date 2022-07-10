// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System.ComponentModel;

namespace osu.Game.Rulesets.UI
{
    public enum PlayfieldBorderStyle
    {
        [Description("无边框")]
        None,
        [Description("显示在4个角落")]
        Corners,
        [Description("全边框")]
        Full
    }
}
