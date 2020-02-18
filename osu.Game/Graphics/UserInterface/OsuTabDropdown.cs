// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabDropdown<T> : OsuDropdown<T>
    {
        public OsuTabDropdown()
        {
            RelativeSizeAxes = Axes.X;
        }

        protected override DropdownMenu CreateMenu() => new OsuTabDropdownMenu();

        protected override DropdownHeader CreateHeader() => new OsuTabDropdownHeader
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight
        };

        private class OsuTabDropdownMenu : OsuDropdownMenu
        {
            public OsuTabDropdownMenu()
            {
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;

                BackgroundColour = Color4.Black.Opacity(0.7f);
                MaxHeight = 400;
            }

            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableOsuTabDropdownMenuItem(item) { AccentColour = AccentColour };

            private class DrawableOsuTabDropdownMenuItem : DrawableOsuDropdownMenuItem
            {
                public DrawableOsuTabDropdownMenuItem(MenuItem item)
                    : base(item)
                {
                    ForegroundColourHover = Color4.Black;
                }
            }
        }

        protected class OsuTabDropdownHeader : OsuDropdownHeader
        {
            public override Color4 AccentColour
            {
                get => base.AccentColour;
                set
                {
                    base.AccentColour = value;
                    Foreground.Colour = value;
                }
            }

            public OsuTabDropdownHeader()
            {
                RelativeSizeAxes = Axes.None;
                AutoSizeAxes = Axes.X;

                BackgroundColour = Color4.Black.Opacity(0.5f);

                Background.Height = 0.5f;
                Background.CornerRadius = 5;
                Background.Masking = true;

                Foreground.RelativeSizeAxes = Axes.None;
                Foreground.AutoSizeAxes = Axes.X;
                Foreground.RelativeSizeAxes = Axes.Y;
                Foreground.Margin = new MarginPadding(5);

                Foreground.Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.EllipsisH,
                        Size = new Vector2(14),
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                    }
                };

                Padding = new MarginPadding { Left = 5, Right = 5 };
            }

            protected override bool OnHover(HoverEvent e)
            {
                Foreground.Colour = BackgroundColour;
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Foreground.Colour = BackgroundColourHover;
                base.OnHoverLost(e);
            }
        }
    }
}
