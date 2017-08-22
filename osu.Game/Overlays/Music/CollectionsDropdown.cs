// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Music
{
    public class CollectionsDropdown<T> : OsuDropdown<T>
    {
        protected override DropdownHeader CreateHeader() => new CollectionsHeader { AccentColour = AccentColour };
        protected override Menu CreateMenu() => new CollectionsMenu();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Gray6;
        }

        private class CollectionsHeader : OsuDropdownHeader
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Gray4;
            }

            public CollectionsHeader()
            {
                CornerRadius = 5;
                Height = 30;
                Icon.Size = new Vector2(14);
                Icon.Margin = new MarginPadding(0);
                Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 10, Right = 10 };
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.3f),
                    Radius = 3,
                    Offset = new Vector2(0f, 1f),
                };
            }
        }

        private class CollectionsMenu : OsuMenu
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Background.Colour = colours.Gray4;
            }

            public CollectionsMenu()
            {
                CornerRadius = 5;
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.3f),
                    Radius = 3,
                    Offset = new Vector2(0f, 1f),
                };
            }
        }
    }
}
