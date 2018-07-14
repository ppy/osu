// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class OsuSetupEnumDropdown<T> : OsuEnumDropdown<T>, IHasAccentColour
    {
        public const float LABEL_CONTAINER_WIDTH = 150;
        public const float OUTER_CORNER_RADIUS = 15;
        public const float INNER_CORNER_RADIUS = 10;
        public const float DEFAULT_HEADER_TEXT_SIZE = 20;
        public const float DEFAULT_HEIGHT = 40;
        public const float DEFAULT_LABEL_TEXT_SIZE = 16;
        public const float DEFAULT_LEFT_PADDING = 15;
        public const float DEFAULT_TOP_PADDING = 12;
        public const float DEFAULT_HEADER_TEXT_PADDING = 11;
        public const float DEFAULT_HEADER_ICON_PADDING = 10;

        public OsuSetupEnumDropdown()
        {
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

        }
        
        protected override DropdownHeader CreateHeader() => new OsuSetupDropdownHeader();

        protected override DropdownMenu CreateMenu() => new OsuSetupDropdownMenu();

        protected class OsuSetupDropdownMenu : OsuDropdownMenu
        {
            public OsuSetupDropdownMenu()
            {
                CornerRadius = INNER_CORNER_RADIUS;
            }
        }

        protected class OsuSetupDropdownHeader : OsuDropdownHeader
        {
            public OsuSetupDropdownHeader()
            {
                Height = DEFAULT_HEIGHT;
                CornerRadius = INNER_CORNER_RADIUS;
                Text.TextSize = DEFAULT_HEADER_TEXT_SIZE;
                Text.Padding = new MarginPadding { Left = DEFAULT_HEADER_TEXT_PADDING };
                Icon.Margin = new MarginPadding { Right = DEFAULT_HEADER_ICON_PADDING };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour osuColour)
            {
                TextColour = osuColour.Blue;
                AccentColour = osuColour.BlueDarker.Darken(0.6f);
            }
        }
    }
}
