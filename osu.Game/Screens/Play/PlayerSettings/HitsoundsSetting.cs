// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public enum HitsoundsSetting
    {
        [Description("Global")]
        UseGlobalSetting = 0,

        [Description("On")]
        HitsoundsOn = 1,

        [Description("Off")]
        HitsoundsOff = 2,
    }
}
