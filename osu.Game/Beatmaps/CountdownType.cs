// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// The type of countdown shown before the start of gameplay on a given beatmap.
    /// </summary>
    public enum CountdownType
    {
        None = 0,

        [Description("Normal")]
        Normal = 1,

        [Description("Half speed")]
        HalfSpeed = 2,

        [Description("Double speed")]
        DoubleSpeed = 3
    }
}
