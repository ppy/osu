﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Overlays.SearchableList
{
    public class SlimEnumDropdown<T> : OsuEnumDropdown<T>
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
