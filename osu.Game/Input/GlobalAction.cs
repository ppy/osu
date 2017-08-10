// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Input
{
    public enum GlobalAction
    {
        [Description("Toggle chat overlay")]
        ToggleChat,
        [Description("Toggle social overlay")]
        ToggleSocial,
        [Description("Reset input settings")]
        ResetInputSettings,
        [Description("Toggle toolbar")]
        ToggleToolbar,
        [Description("Toggle settings")]
        ToggleSettings,
        [Description("Toggle osu!direct")]
        ToggleDirect,
    }
}
