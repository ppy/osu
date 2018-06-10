// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Screens.Edit.Screens.Setup
{
    public enum SetupScreenMode
    {
        [Description("General")]
        General,
        [Description("Difficulty")]
        Difficulty,
        [Description("Audio")]
        Audio,
        [Description("Colours")]
        Colours,
        [Description("Design")]
        Design,
        [Description("Advanced")]
        Advanced
    }
}
