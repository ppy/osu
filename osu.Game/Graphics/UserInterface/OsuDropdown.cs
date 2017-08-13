// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuDropdown<T> : Dropdown<T>
    {
        protected override DropdownHeader CreateHeader() => new OsuDropdownHeader { AccentColour = AccentColour };

        protected override Menu CreateMenu() => new OsuMenu();

        private Color4? accentColour;
        public virtual Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                if (Header != null)
                    ((OsuDropdownHeader)Header).AccentColour = value;
                foreach (var item in MenuItems.OfType<OsuDropdownMenuItem>())
                    item.AccentColour = value;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.PinkDarker;
        }

        protected override DropdownMenuItem<T> CreateMenuItem(string text, T value) => new OsuDropdownMenuItem(text, value) { AccentColour = AccentColour };

        public class OsuDropdownMenuItem : DropdownMenuItem<T>
        {
            public OsuDropdownMenuItem(string text, T current) : base(text, current)
            {
                Foreground.Padding = new MarginPadding(2);

                Masking = true;
                CornerRadius = 6;

                Children = new[]
                {
                new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        Chevron = new SpriteIcon
                        {
                            AlwaysPresent = true,
                            Icon = FontAwesome.fa_chevron_right,
                            Colour = Color4.Black,
                            Alpha = 0.5f,
                            Size = new Vector2(8),
                            Margin = new MarginPadding { Left = 3, Right = 3 },
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                        },
                        Label = new OsuSpriteText {
                            Text = text,
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                        }
                    }
                }
            };
            }

            private Color4? accentColour;

            protected readonly SpriteIcon Chevron;
            protected readonly OsuSpriteText Label;

            protected override void FormatForeground(bool hover = false)
            {
                base.FormatForeground(hover);
                Chevron.Alpha = hover ? 1 : 0;
            }

            public Color4 AccentColour
            {
                get { return accentColour.GetValueOrDefault(); }
                set
                {
                    accentColour = value;
                    BackgroundColourHover = BackgroundColourSelected = value;
                    FormatBackground();
                    FormatForeground();
                }
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = Color4.Transparent;
                BackgroundColourHover = accentColour ?? colours.PinkDarker;
                BackgroundColourSelected = Color4.Black.Opacity(0.5f);
            }
        }

        public class OsuDropdownHeader : DropdownHeader
        {
            protected readonly SpriteText Text;
            protected override string Label
            {
                get { return Text.Text; }
                set { Text.Text = value; }
            }

            protected readonly SpriteIcon Icon;

            private Color4? accentColour;
            public virtual Color4 AccentColour
            {
                get { return accentColour.GetValueOrDefault(); }
                set
                {
                    accentColour = value;
                    BackgroundColourHover = value;
                }
            }

            public OsuDropdownHeader()
            {
                Foreground.Padding = new MarginPadding(4);

                AutoSizeAxes = Axes.None;
                Margin = new MarginPadding { Bottom = 4 };
                CornerRadius = 4;
                Height = 40;

                Foreground.Children = new Drawable[]
                {
                    Text = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    Icon = new SpriteIcon
                    {
                        Icon = FontAwesome.fa_chevron_down,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Margin = new MarginPadding { Right = 4 },
                        Size = new Vector2(20),
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = Color4.Black.Opacity(0.5f);
                BackgroundColourHover = accentColour ?? colours.PinkDarker;
            }
        }
    }
}
