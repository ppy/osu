// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class OverlayTabControl<T> : TabControl<T>
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

        public OverlayTabControl()
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

        protected override TabItem<T> CreateTabItem(T value) => new OverlayTabItem<T>(value)
        {
            AccentColour = AccentColour
        };

        protected class OverlayTabItem<U> : TabItem<U>
        {
            private readonly ExpandingBar bar;

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
                    OnActivated();

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                if (!Active.Value)
                    OnDeactivated();
            }

            protected override void OnActivated() => bar.Expand();

            protected override void OnDeactivated() => bar.Collapse();

            private void updateState()
            {
                if (Active.Value)
                    OnActivated();
                else
                    OnDeactivated();
            }
        }
    }
}
