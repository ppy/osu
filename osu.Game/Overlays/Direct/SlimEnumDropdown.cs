// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Direct
{
    public class SlimEnumDropdown<T> : OsuEnumDropdown<T>
    {
        public const float HEIGHT = 25;

        protected override DropdownHeader CreateHeader() => new SlimDropdownHeader { AccentColour = AccentColour };
        protected override Menu CreateMenu() => new SlimMenu();

        private class SlimDropdownHeader : OsuDropdownHeader
        {
            public SlimDropdownHeader()
            {
                Height = HEIGHT;
                Icon.TextSize = 16;
                Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 4 };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                BackgroundColour = Color4.Black.Opacity(0.25f);
            }
        }

        private class SlimMenu : OsuMenu
        {
            public SlimMenu()
            {
                Background.Colour = Color4.Black.Opacity(0.25f);
            }
        }
    }
}
