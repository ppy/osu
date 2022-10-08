// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuEnumDropdown<T> : OsuDropdown<T>
        where T : struct, Enum
    {
        public OsuEnumDropdown()
        {
            Items = (T[])Enum.GetValues(typeof(T));
        }
    }
}
