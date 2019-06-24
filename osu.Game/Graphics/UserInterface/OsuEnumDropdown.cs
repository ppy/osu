// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuEnumDropdown<T> : OsuDropdown<T>
    {
        public OsuEnumDropdown()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("OsuEnumDropdown only supports enums as the generic type argument");

            Items = (T[])Enum.GetValues(typeof(T));
        }
    }
}
