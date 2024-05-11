// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Graphics.UserInterface
{
    public partial class OsuEnumDropdown<T> : OsuDropdown<T>
        where T : struct, Enum
    {
        public OsuEnumDropdown()
        {
            Items = Enum.GetValues<T>();
        }
    }
}
