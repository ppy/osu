// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class SlimEnumDropdown<T> : OsuEnumDropdown<T>
        where T : struct, Enum
    {
        protected override DropdownHeader CreateHeader() => new SlimDropdownHeader();

        protected override DropdownMenu CreateMenu() => new SlimMenu();

        private class SlimDropdownHeader : OsuDropdownHeader
        {
            public SlimDropdownHeader()
            {
                Height = 25;
                Icon.Size = new Vector2(16);
                Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 4 };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                BackgroundColour = Color4.Black.Opacity(0.25f);
            }
        }

        private class SlimMenu : OsuDropdownMenu
        {
            public SlimMenu()
            {
                BackgroundColour = Color4.Black.Opacity(0.7f);
            }
        }
    }
}
