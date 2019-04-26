// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
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
                if (accentColour == value)
                    return;

                accentColour = value;
                bar.Colour = value;

                foreach (TabItem<string> tabItem in TabContainer)
                {
                    ((ProfileHeaderTabItem)tabItem).AccentColour = value;
                }
            }
        }

        public new MarginPadding Padding
        {
            get => TabContainer.Padding;
            set => TabContainer.Padding = value;
        }

        public ProfileHeaderTabControl()
        {
            TabContainer.Masking = false;
            TabContainer.Spacing = new Vector2(15, 0);

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
                    if (accentColour == value)
                        return;

                    accentColour = value;
                    bar.Colour = value;

                    updateState();
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
                        Margin = new MarginPadding { Bottom = 10 },
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        Text = value,
                        Font = OsuFont.GetFont()
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
                base.OnHover(e);

                updateState();

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                updateState();
            }

            protected override void OnActivated() => updateState();

            protected override void OnDeactivated() => updateState();

            private void updateState()
            {
                if (Active.Value || IsHovered)
                {
                    text.FadeColour(Color4.White, 120, Easing.InQuad);
                    bar.ResizeHeightTo(7.5f, 120, Easing.InQuad);

                    if (Active.Value)
                        text.Font = text.Font.With(weight: FontWeight.Bold);
                }
                else
                {
                    text.FadeColour(AccentColour, 120, Easing.InQuad);
                    bar.ResizeHeightTo(0, 120, Easing.InQuad);
                    text.Font = text.Font.With(weight: FontWeight.Medium);
                }
            }
        }
    }
}
