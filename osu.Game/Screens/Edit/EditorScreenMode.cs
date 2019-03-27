// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.Edit
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
