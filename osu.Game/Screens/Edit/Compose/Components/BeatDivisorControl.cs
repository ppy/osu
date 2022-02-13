// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class BeatDivisorControl : CompositeDrawable
    {
        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();
        private readonly Bindable<BeatDivisorType> divisorType = new Bindable<BeatDivisorType>();

        public BeatDivisorControl(BindableBeatDivisor beatDivisor)
        {
            this.beatDivisor.BindTo(beatDivisor);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Name = "Gray Background",
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray4
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Name = "Black Background",
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black
                                    },
                                    new TickSliderBar(beatDivisor, BindableBeatDivisor.VALID_DIVISORS)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colours.Gray4
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Child = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new ChevronButton
                                                    {
                                                        Icon = FontAwesome.Solid.ChevronLeft,
                                                        Action = beatDivisor.Previous
                                                    },
                                                    new DivisorText { BeatDivisor = { BindTarget = beatDivisor } },
                                                    new ChevronButton
                                                    {
                                                        Icon = FontAwesome.Solid.ChevronRight,
                                                        Action = beatDivisor.Next
                                                    }
                                                },
                                            },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.Absolute, 20),
                                                new Dimension(),
                                                new Dimension(GridSizeMode.Absolute, 20)
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new TextFlowContainer(s => s.Font = s.Font.With(size: 14))
                            {
                                Padding = new MarginPadding { Horizontal = 15 },
                                Text = "beat snap",
                                RelativeSizeAxes = Axes.X,
                                TextAnchor = Anchor.TopCentre
                            },
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colours.Gray4
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Child = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new ChevronButton
                                                    {
                                                        Icon = FontAwesome.Solid.ChevronLeft,
                                                        Action = () => cycleDivisorType(-1)
                                                    },
                                                    new DivisorTypeText { BeatDivisorType = { BindTarget = divisorType } },
                                                    new ChevronButton
                                                    {
                                                        Icon = FontAwesome.Solid.ChevronRight,
                                                        Action = () => cycleDivisorType(1)
                                                    }
                                                },
                                            },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.Absolute, 20),
                                                new Dimension(),
                                                new Dimension(GridSizeMode.Absolute, 20)
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 30),
                        new Dimension(GridSizeMode.Absolute, 20),
                        new Dimension(GridSizeMode.Absolute, 15)
                    }
                }
            };
        }

        private void cycleDivisorType(int direction)
        {
            Debug.Assert(Math.Abs(direction) == 1);
            divisorType.Value = (BeatDivisorType)(((int)divisorType.Value + direction) % (int)(BeatDivisorType.Last + 1));
        }

        private class DivisorText : SpriteText
        {
            public Bindable<int> BeatDivisor { get; } = new Bindable<int>();

            public DivisorText()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = colours.BlueLighter;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                BeatDivisor.BindValueChanged(val => Text = $"1/{val.NewValue}", true);
            }
        }

        private class DivisorTypeText : OsuSpriteText
        {
            public Bindable<BeatDivisorType> BeatDivisorType { get; } = new Bindable<BeatDivisorType>();

            public DivisorTypeText()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Font = OsuFont.Default.With(size: 14);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                BeatDivisorType.BindValueChanged(val => Text = val.NewValue.Humanize(LetterCasing.LowerCase), true);
            }
        }

        private class ChevronButton : IconButton
        {
            public ChevronButton()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                // Small offset to look a bit better centered along with the divisor text
                Y = 1;

                Size = new Vector2(20);
                IconScale = new Vector2(0.6f);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IconColour = Color4.Black;
                HoverColour = colours.Gray7;
                FlashColour = colours.Gray9;
            }
        }

        private class TickSliderBar : SliderBar<int>
        {
            private Marker marker;

            [Resolved]
            private OsuColour colours { get; set; }

            private readonly BindableBeatDivisor beatDivisor;
            private readonly int[] availableDivisors;

            public TickSliderBar(BindableBeatDivisor beatDivisor, params int[] divisors)
            {
                CurrentNumber.BindTo(this.beatDivisor = beatDivisor);
                availableDivisors = divisors;

                Padding = new MarginPadding { Horizontal = 5 };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                foreach (int t in availableDivisors)
                {
                    AddInternal(new Tick
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.X,
                        Colour = BindableBeatDivisor.GetColourFor(t, colours),
                        X = getMappedPosition(t)
                    });
                }

                AddInternal(marker = new Marker());
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                CurrentNumber.BindValueChanged(div =>
                {
                    marker.MoveToX(getMappedPosition(div.NewValue), 100, Easing.OutQuint);
                    marker.Flash();
                }, true);
            }

            protected override void UpdateValue(float value)
            {
            }

            public override bool HandleNonPositionalInput => IsHovered && !CurrentNumber.Disabled;

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                switch (e.Key)
                {
                    case Key.Right:
                        beatDivisor.Next();
                        OnUserChange(Current.Value);
                        return true;

                    case Key.Left:
                        beatDivisor.Previous();
                        OnUserChange(Current.Value);
                        return true;

                    default:
                        return false;
                }
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                marker.Active = true;
                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                marker.Active = false;
                base.OnMouseUp(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                handleMouseInput(e.ScreenSpaceMousePosition);
                return true;
            }

            protected override void OnDrag(DragEvent e)
            {
                handleMouseInput(e.ScreenSpaceMousePosition);
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                handleMouseInput(e.ScreenSpaceMousePosition);
            }

            private void handleMouseInput(Vector2 screenSpaceMousePosition)
            {
                // copied from SliderBar so we can do custom spacing logic.
                float xPosition = (ToLocalSpace(screenSpaceMousePosition).X - RangePadding) / UsableWidth;

                CurrentNumber.Value = availableDivisors.OrderBy(d => Math.Abs(getMappedPosition(d) - xPosition)).First();
                OnUserChange(Current.Value);
            }

            private float getMappedPosition(float divisor) => MathF.Pow((divisor - 1) / (availableDivisors.Last() - 1), 0.90f);

            private class Tick : CompositeDrawable
            {
                public Tick()
                {
                    Size = new Vector2(2.5f, 10);

                    InternalChild = new Box { RelativeSizeAxes = Axes.Both };

                    CornerRadius = 0.5f;
                    Masking = true;
                }
            }

            private class Marker : CompositeDrawable
            {
                private Color4 defaultColour;

                private const float size = 7;

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Colour = defaultColour = colours.Gray4;
                    Anchor = Anchor.TopLeft;
                    Origin = Anchor.TopCentre;

                    Width = size;
                    RelativeSizeAxes = Axes.Y;
                    RelativePositionAxes = Axes.X;

                    InternalChildren = new Drawable[]
                    {
                        new Box
                        {
                            Width = 2,
                            RelativeSizeAxes = Axes.Y,
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre,
                            Colour = ColourInfo.GradientVertical(Color4.White.Opacity(0.2f), Color4.White),
                            Blending = BlendingParameters.Additive,
                        },
                        new EquilateralTriangle
                        {
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre,
                            Height = size,
                            EdgeSmoothness = new Vector2(1),
                            Colour = Color4.White,
                        }
                    };
                }

                private bool active;

                public bool Active
                {
                    get => active;
                    set
                    {
                        this.FadeColour(value ? Color4.White : defaultColour, 500, Easing.OutQuint);
                        active = value;
                    }
                }

                public void Flash()
                {
                    bool wasActive = active;

                    Active = true;

                    if (wasActive) return;

                    using (BeginDelayedSequence(50))
                        Active = false;
                }
            }
        }
    }
}
