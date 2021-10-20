// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Login
{
    public class UserDropdown : OsuEnumDropdown<UserAction>
    {
        protected override DropdownHeader CreateHeader() => new UserDropdownHeader();

        protected override DropdownMenu CreateMenu() => new UserDropdownMenu();

        public Color4 StatusColour
        {
            set
            {
                if (Header is UserDropdownHeader h)
                    h.StatusColour = value;
            }
        }

        protected class UserDropdownMenu : OsuDropdownMenu
        {
            public UserDropdownMenu()
            {
                Masking = true;
                CornerRadius = 5;

                Margin = new MarginPadding { Bottom = 5 };

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.25f),
                    Radius = 4,
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Gray3;
                SelectionColour = colours.Gray4;
                HoverColour = colours.Gray5;
            }

            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableUserDropdownMenuItem(item);

            private class DrawableUserDropdownMenuItem : DrawableOsuDropdownMenuItem
            {
                public DrawableUserDropdownMenuItem(MenuItem item)
                    : base(item)
                {
                    Foreground.Padding = new MarginPadding { Top = 5, Bottom = 5, Left = 10, Right = 5 };
                    CornerRadius = 5;
                }

                protected override Drawable CreateContent() => new Content
                {
                    Label = { Margin = new MarginPadding { Left = UserDropdownHeader.LABEL_LEFT_MARGIN - 11 } }
                };
            }
        }

        private class UserDropdownHeader : OsuDropdownHeader
        {
            public const float LABEL_LEFT_MARGIN = 20;

            private readonly SpriteIcon statusIcon;

            public Color4 StatusColour
            {
                set => statusIcon.FadeColour(value, 500, Easing.OutQuint);
            }

            public UserDropdownHeader()
            {
                Foreground.Padding = new MarginPadding { Left = 10, Right = 10 };
                Margin = new MarginPadding { Bottom = 5 };
                Masking = true;
                CornerRadius = 5;
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.25f),
                    Radius = 4,
                };

                Icon.Size = new Vector2(14);
                Icon.Margin = new MarginPadding(0);

                Foreground.Add(statusIcon = new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Icon = FontAwesome.Regular.Circle,
                    Size = new Vector2(14),
                });

                Text.Margin = new MarginPadding { Left = LABEL_LEFT_MARGIN };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Gray3;
                BackgroundColourHover = colours.Gray5;
            }
        }
    }
}
