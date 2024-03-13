// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public enum LegacyNoteBodyStyle
    {
        Stretch = 0,

        // listed as the default on https://osu.ppy.sh/wiki/en/Skinning/skin.ini, but is seemingly not according to the source.
        // Repeat = 1,

        RepeatTop = 2,
        RepeatBottom = 3,
        RepeatTopAndBottom = 4,
    }
}
