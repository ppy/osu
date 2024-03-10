// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterface
{
    public partial class RoundedSliderBar<T> : OsuSliderBar<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        protected readonly Nub Nub;
        protected readonly Box LeftBox;
        protected readonly Box RightBox;
        private readonly Container nubContainer;

        private readonly HoverClickSounds hoverClickSounds;

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                LeftBox.Colour = value;
            }
        }

        private Colour4 backgroundColour;

        public Color4 BackgroundColour
        {
            get => backgroundColour;
            set
            {
                backgroundColour = value;
                RightBox.Colour = value;
            }
        }

        public RoundedSliderBar()
        {
            Height = Nub.HEIGHT;
            RangePadding = Nub.DEFAULT_EXPANDED_SIZE / 2;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding { Horizontal = 2 },
                    Child = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Masking = true,
                        CornerRadius = 5f,
                        Children = new Drawable[]
                        {
                            LeftBox = new Box
                            {
                                Height = 5,
                                EdgeSmoothness = new Vector2(0, 0.5f),
                                RelativeSizeAxes = Axes.None,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            RightBox = new Box
                            {
                                Height = 5,
                                EdgeSmoothness = new Vector2(0, 0.5f),
                                RelativeSizeAxes = Axes.None,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                            },
                        },
                    },
                },
                nubContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = Nub = new SliderNub
                    {
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.X,
                        Current = { Value = true },
                        OnDoubleClicked = () =>
                        {
                            if (!Current.Disabled)
                                Current.SetDefault();
                        },
                    },
                },
                hoverClickSounds = new HoverClickSounds()
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider, OsuColour colours)
        {
            AccentColour = colourProvider?.Highlight1 ?? colours.Pink;
            BackgroundColour = colourProvider?.Background5 ?? colours.PinkDarker.Darken(1);
        }

        protected override void Update()
        {
            base.Update();

            nubContainer.Padding = new MarginPadding { Horizontal = RangePadding };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindDisabledChanged(disabled =>
            {
                Alpha = disabled ? 0.3f : 1;
                hoverClickSounds.Enabled.Value = !disabled;
            }, true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateGlow();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateGlow();
            base.OnHoverLost(e);
        }

        protected override bool ShouldHandleAsRelativeDrag(MouseDownEvent e)
            => Nub.ReceivePositionalInputAt(e.ScreenSpaceMouseDownPosition);

        protected override void OnDragEnd(DragEndEvent e)
        {
            updateGlow();
            base.OnDragEnd(e);
        }

        private void updateGlow()
        {
            Nub.Glowing = !Current.Disabled && (IsHovered || IsDragged);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            LeftBox.Scale = new Vector2(Math.Clamp(RangePadding + Nub.DrawPosition.X - Nub.DrawWidth / 2, 0, Math.Max(0, DrawWidth)), 1);
            RightBox.Scale = new Vector2(Math.Clamp(DrawWidth - Nub.DrawPosition.X - RangePadding - Nub.DrawWidth / 2, 0, Math.Max(0, DrawWidth)), 1);
        }

        protected override void UpdateValue(float value)
        {
            Nub.MoveToX(value, 250, Easing.OutQuint);
        }

        public partial class SliderNub : Nub
        {
            public Action? OnDoubleClicked { get; init; }

            protected override bool OnClick(ClickEvent e) => true;

            protected override bool OnDoubleClick(DoubleClickEvent e)
            {
                OnDoubleClicked?.Invoke();
                return true;
            }
        }
    }
}
