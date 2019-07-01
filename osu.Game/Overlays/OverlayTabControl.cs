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

namespace osu.Game.Overlays
{
    public abstract class OverlayTabControl<T> : TabControl<T>
    {
        private readonly Box bar;

        private Color4 accentColour = Color4.White;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value)
                    return;

                accentColour = value;
                bar.Colour = value;

                foreach (TabItem<T> tabItem in TabContainer)
                {
                    ((OverlayTabItem<T>)tabItem).AccentColour = value;
                }
            }
        }

        public new MarginPadding Padding
        {
            get => TabContainer.Padding;
            set => TabContainer.Padding = value;
        }

        protected OverlayTabControl()
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

        protected override Dropdown<T> CreateDropdown() => null;

        protected override TabItem<T> CreateTabItem(T value) => new OverlayTabItem<T>(value);

        protected class OverlayTabItem<U> : TabItem<U>
        {
            private readonly ExpandingBar bar;

            protected readonly OsuSpriteText Text;

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

            public OverlayTabItem(U value)
                : base(value)
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                Children = new Drawable[]
                {
                    Text = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Bottom = 10 },
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        Font = OsuFont.GetFont(),
                    },
                    bar = new ExpandingBar
                    {
                        Anchor = Anchor.BottomCentre,
                        ExpandedSize = 7.5f,
                        CollapsedSize = 0
                    },
                    new HoverClickSounds()
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);

                if (!Active.Value)
                    HoverAction();

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                if (!Active.Value)
                    UnhoverAction();
            }

            protected override void OnActivated()
            {
                HoverAction();
                Text.Font = Text.Font.With(weight: FontWeight.Bold);
            }

            protected override void OnDeactivated()
            {
                UnhoverAction();
                Text.Font = Text.Font.With(weight: FontWeight.Medium);
            }

            private void updateState()
            {
                if (Active.Value)
                    OnActivated();
                else
                    OnDeactivated();
            }

            protected virtual void HoverAction()
            {
                bar.Expand();
                Text.FadeColour(Color4.White, 120, Easing.InQuad);
            }

            protected virtual void UnhoverAction()
            {
                bar.Collapse();
                Text.FadeColour(AccentColour, 120, Easing.InQuad);
            }
        }
    }
}
