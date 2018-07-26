// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class SetupEnumDropdown<T> : OsuDropdown<T>
    {
        private OsuSetupDropdownHeader header;
        private OsuSetupDropdownMenu menu;

        private const float default_corner_radius = 15;
        private const float default_height = 40;
        private const float default_header_icon_padding = 15;
        private const float default_header_text_padding = 15;
        private const float default_header_text_size = 20;

        public SetupEnumDropdown()
        {
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            header.AccentColour = osuColour.BlueDarker.Darken(0.6f);
            menu.AccentColour = osuColour.BlueDarker.Darken(0.6f);
        }

        protected override DropdownHeader CreateHeader() => header = new OsuSetupDropdownHeader();

        protected override DropdownMenu CreateMenu() => menu = new OsuSetupDropdownMenu();

        protected class OsuSetupDropdownMenu : OsuDropdownMenu
        {
            public OsuSetupDropdownMenu()
            {
                CornerRadius = default_corner_radius;
            }
        }

        protected class OsuSetupDropdownHeader : OsuDropdownHeader
        {
            public Color4 TextColour
            {
                get => Text.Colour;
                set => Text.Colour = value;
            }

            public OsuSetupDropdownHeader()
            {
                Height = default_height;
                CornerRadius = default_corner_radius;
                Text.TextSize = default_header_text_size;
                Foreground.Padding = new MarginPadding { Left = default_header_text_padding };
                Icon.Margin = new MarginPadding { Right = default_header_icon_padding };
            }
        }
    }
}
