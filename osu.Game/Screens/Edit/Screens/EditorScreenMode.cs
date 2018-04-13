// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Screens.Edit.Screens
{
    public enum EditorScreenMode
    {
        [Description("setup")]
        SongSetup,
        [Description("compose")]
        Compose,
        [Description("design")]
        Design,
        [Description("timing")]
        Timing,
    }
}
