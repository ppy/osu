// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.Edit
{
    public enum EditorScreenMode
    {
        [Description("谱面设置")]
        SongSetup,

        [Description("物件排布")]
        Compose,

        [Description("谱面设计")]
        Design,

        [Description("timing设计")]
        Timing,

        [Description("verify")]
        Verify,
    }
}
