// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum HUDVisibilityMode
    {
        [Description("从不显示")]
        Never,

        [Description("在非游玩时段显示")]
        HideDuringGameplay,

        [Description("总是显示")]
        Always
    }
}
