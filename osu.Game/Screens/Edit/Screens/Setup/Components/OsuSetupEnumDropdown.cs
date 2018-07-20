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
    public class OsuSetupEnumDropdown<T> : OsuEnumDropdown<T>
    {
        private OsuSetupDropdownHeader header;
        private OsuSetupDropdownMenu menu;

        public const float DEFAULT_CORNER_RADIUS = 10;
        public const float DEFAULT_HEIGHT = 40;
        public const float DEFAULT_HEADER_ICON_PADDING = 10;
        public const float DEFAULT_HEADER_TEXT_PADDING = 11;
        public const float DEFAULT_HEADER_TEXT_SIZE = 20;

        public OsuSetupEnumDropdown()
        {
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            header.TextColour = osuColour.Blue;
            header.AccentColour = osuColour.BlueDarker.Darken(0.6f);
            menu.AccentColour = osuColour.BlueDarker.Darken(0.6f);
        }

        protected override DropdownHeader CreateHeader() => header = new OsuSetupDropdownHeader();

        protected override DropdownMenu CreateMenu() => menu = new OsuSetupDropdownMenu();

        protected class OsuSetupDropdownMenu : OsuDropdownMenu
        {
            public OsuSetupDropdownMenu()
            {
                CornerRadius = DEFAULT_CORNER_RADIUS;
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
                Height = DEFAULT_HEIGHT;
                CornerRadius = DEFAULT_CORNER_RADIUS;
                Text.TextSize = DEFAULT_HEADER_TEXT_SIZE;
                Text.Padding = new MarginPadding { Left = DEFAULT_HEADER_TEXT_PADDING };
                Icon.Margin = new MarginPadding { Right = DEFAULT_HEADER_ICON_PADDING };
            }
        }
    }
}
