// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
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
    public abstract class OverlayTabControl<T> : OsuTabControl<T>
    {
        private readonly Box bar;

        protected float BarHeight
        {
            set => bar.Height = value;
        }

        public override Color4 AccentColour
        {
            get => base.AccentColour;
            set
            {
                base.AccentColour = value;
                bar.Colour = value;
            }
        }

        protected OverlayTabControl()
        {
            TabContainer.Masking = false;
            TabContainer.Spacing = new Vector2(15, 0);

            AddInternal(bar = new Box
            {
                RelativeSizeAxes = Axes.X,
                Margin = new MarginPadding { Top = -1 },
                Height = 2,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.CentreLeft
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AccentColour = colourProvider.Highlight1;
        }

        protected override Dropdown<T> CreateDropdown() => null;

        protected override TabItem<T> CreateTabItem(T value) => new OverlayTabItem(value);

        protected class OverlayTabItem : TabItem<T>, IHasAccentColour
        {
            protected readonly ExpandingBar Bar;
            protected readonly OsuSpriteText Text;

            public Color4 AccentColour
            {
                get => Bar.Colour;
                set => Bar.Colour = value;
            }

            private Color4 idleColour;

            protected Color4 IdleColour
            {
                get => idleColour;
                set
                {
                    if (idleColour == value)
                        return;

                    idleColour = value;
                    updateState();
                }
            }

            public OverlayTabItem(T value)
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
                    Bar = new ExpandingBar
                    {
                        Anchor = Anchor.BottomCentre,
                        ExpandedSize = 7.5f,
                        CollapsedSize = 0,
                        Margin = new MarginPadding { Top = -1 }
                    },
                    new HoverClickSounds()
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Light2;
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

            protected override void OnActivated() => HoverAction();

            protected override void OnDeactivated() => UnhoverAction();

            private void updateState()
            {
                if (Active.Value)
                    OnActivated();
                else
                    OnDeactivated();
            }

            protected virtual void HoverAction()
            {
                Bar.Expand();
                Text.FadeColour(Color4.White, 120, Easing.InQuad);
            }

            protected virtual void UnhoverAction()
            {
                Bar.Collapse();
                Text.FadeColour(idleColour, 120, Easing.InQuad);
            }
        }
    }
}
