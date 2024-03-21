// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public partial class SlimEnumDropdown<T> : OsuEnumDropdown<T>
        where T : struct, Enum
    {
        protected override DropdownHeader CreateHeader() => new SlimDropdownHeader();

        private partial class SlimDropdownHeader : OsuDropdownHeader
        {
            public SlimDropdownHeader()
            {
                Height = 25;
                Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 4 };
            }
        }
    }
}
