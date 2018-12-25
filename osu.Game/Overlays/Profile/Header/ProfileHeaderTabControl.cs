// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header
{
    public class ProfileHeaderTabControl : TabControl<string>
    {
        private readonly Box bar;

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value) return;

                accentColour = value;

                bar.Colour = value;

                foreach (TabItem<string> tabItem in TabContainer)
                {
                    ((ProfileHeaderTabItem)tabItem).AccentColour = value;
                }
            }
        }

        public MarginPadding Padding
        {
            get => TabContainer.Padding;
            set => TabContainer.Padding = value;
        }

        public ProfileHeaderTabControl()
        {
            TabContainer.Masking = false;
            TabContainer.Spacing = new Vector2(20, 0);

            AddInternal(bar = new Box
            {
                RelativeSizeAxes = Axes.X,
                Height = 2,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.CentreLeft
            });
        }

        protected override Dropdown<string> CreateDropdown() => null;

        protected override TabItem<string> CreateTabItem(string value) => new ProfileHeaderTabItem(value)
        {
            AccentColour = AccentColour
        };

        private class ProfileHeaderTabItem : TabItem<string>
        {
            private readonly OsuSpriteText text;
            private readonly Drawable bar;

            private Color4 accentColour;

            public Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    accentColour = value;

                    bar.Colour = value;
                    if (!Active) text.Colour = value;
                }
            }

            public ProfileHeaderTabItem(string value)
                : base(value)
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                Children = new[]
                {
                    text = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Bottom = 15 },
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        Text = value,
                        TextSize = 14,
                        Font = "Exo2.0-Bold",
                    },
                    bar = new Circle
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 0,
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.BottomLeft,
                    },
                    new HoverClickSounds()
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (!Active)
                    onActivated(true);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                if (!Active)
                    OnDeactivated();
            }

            protected override void OnActivated()
            {
                onActivated();
            }

            protected override void OnDeactivated()
            {
                text.FadeColour(AccentColour, 120, Easing.InQuad);
                bar.ResizeHeightTo(0, 120, Easing.InQuad);
                text.Font = "Exo2.0-Medium";
            }

            private void onActivated(bool fake = false)
            {
                text.FadeColour(Color4.White, 120, Easing.InQuad);
                bar.ResizeHeightTo(7.5f, 120, Easing.InQuad);
                if (!fake)
                    text.Font = "Exo2.0-Bold";
            }
        }
    }
}
