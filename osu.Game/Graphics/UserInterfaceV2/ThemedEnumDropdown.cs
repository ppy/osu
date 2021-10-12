// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class ThemedEnumDropdown<T> : ThemedDropdown<T>
        where T : struct, Enum
    {
        public ThemedEnumDropdown()
        {
            Items = (T[])Enum.GetValues(typeof(T));
        }
    }
}
