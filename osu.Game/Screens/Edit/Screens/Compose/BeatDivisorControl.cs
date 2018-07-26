// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Screens.Edit.Screens.Compose
{
    public class BeatDivisorControl : CompositeDrawable
    {
        private DivisorPanelText panelText;
        private DivisorText divisorText;
        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

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
                                    new TickSliderBar(beatDivisor)
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
                                        Padding = new MarginPadding { Horizontal = 5 },
                                        Child = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new DivisorButton
                                                    {
                                                        Icon = FontAwesome.fa_chevron_left,
                                                        Action = beatDivisor.PreviousDivisor
                                                    },
                                                    divisorText = new DivisorText(beatDivisor),
                                                    new DivisorButton
                                                    {
                                                        Icon = FontAwesome.fa_chevron_right,
                                                        Action = beatDivisor.NextDivisor
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
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Horizontal = 5 },
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Horizontal = 5 },
                                        Child = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new TextFlowContainer(s => s.TextSize = 11)
                                                    {
                                                        Padding = new MarginPadding { Horizontal = 0 },
                                                        Text = "beat snap divisor",
                                                        RelativeSizeAxes = Axes.Both,
                                                        TextAnchor = Anchor.TopCentre
                                                    },
                                                },
                                            },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Horizontal = 5 },
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Horizontal = 5 },
                                        Child = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new DivisorButton
                                                    {
                                                        Icon = FontAwesome.fa_chevron_left,
                                                        Action = beatDivisor.PreviousPanel,
                                                        //Anchor = Anchor.BottomRight
                                                    },
                                                    panelText = new DivisorPanelText(beatDivisor),
                                                    new DivisorButton
                                                    {
                                                        Icon = FontAwesome.fa_chevron_right,
                                                        Action = beatDivisor.NextPanel,
                                                        //Anchor = Anchor.BottomRight
                                                    }
                                                },
                                            },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(GridSizeMode.Absolute, 50),
                                                new Dimension()
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 25),
                        new Dimension(GridSizeMode.Absolute, 25),
                        new Dimension(GridSizeMode.Absolute, 11),
                        new Dimension(GridSizeMode.Absolute, 25),
                    }
                }
            };
            beatDivisor.ValueChanged += updateDivisorsText;
            beatDivisor.ValidDivisorsChanged += updatePanelText;
            updateDivisorsText(0);
            updatePanelText();
        }

        private void updateDivisorsText(int a) => divisorText.Text = $"1/{beatDivisor.Value}";
        private void updatePanelText() => panelText.Text = $"{beatDivisor.Panel}";

        private class DivisorPanelText : SpriteText
        {
            private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

            public DivisorPanelText(BindableBeatDivisor bd)
            {
                beatDivisor.BindTo(bd);

                TextSize = 12;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = colours.BlueLighter;
            }
        }
        private class DivisorText : SpriteText
        {
            private readonly Bindable<int> beatDivisor = new Bindable<int>();

            public DivisorText(BindableBeatDivisor bd)
            {
                beatDivisor.BindTo(bd);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = colours.BlueLighter;
            }
        }

        private class DivisorButton : IconButton
        {
            public DivisorButton()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                // Small offset to look a bit better centered along with the divisor text
                Y = 1;

                ButtonSize = new Vector2(20);
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

            private readonly BindableBeatDivisor beatDivisor;

            public TickSliderBar(BindableBeatDivisor bd)
            {
                CurrentNumber.BindTo(beatDivisor = bd);

                Padding = new MarginPadding { Horizontal = 5 };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                //foreach (var t in availableDivisors)
                //{
                //    AddInternal(new Tick(t)
                //    {
                //        Anchor = Anchor.TopLeft,
                //        Origin = Anchor.TopCentre,
                //        RelativePositionAxes = Axes.X,
                //        X = getMappedPosition(t)
                //    });
                //}

                //AddInternal(marker = new Marker());

                //CurrentNumber.ValueChanged += v =>
                //{
                //    marker.MoveToX(getMappedPosition(v), 100, Easing.OutQuint);
                //    marker.Flash();
                //};
            }
            protected override void LoadComplete()
            {
                base.LoadComplete();

                beatDivisor.ValidDivisorsChanged += updateDivisors;
                updateDivisors();
            }

            private void updateDivisors()
            {
                for (int i = Children.Count - 1; i >= 0; i--)
                    RemoveInternal(Children[i]); // Remove all ticks to change the panel
                foreach (var t in beatDivisor.ValidDivisors)
                {
                    AddInternal(new Tick(t)
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.X,
                        X = getMappedPosition(t)
                    });
                }
                AddInternal(marker = new Marker());
                CurrentNumber.ValueChanged += moveMarker;
                moveMarker(CurrentNumber.Value);
            }

            private void moveMarker(int v)
            {
                marker.MoveToX(getMappedPosition(v), 100, Easing.OutQuint);
                marker.Flash();
            }

            protected override void UpdateValue(float value)
            {
            }

            public override bool HandleKeyboardInput => IsHovered && !CurrentNumber.Disabled;

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                switch (args.Key)
                {
                    case Key.Right:
                        beatDivisor.NextDivisor();
                        OnUserChange();
                        return true;
                    case Key.Left:
                        beatDivisor.PreviousDivisor();
                        OnUserChange();
                        return true;
                    default:
                        return false;
                }
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                marker.Active = true;
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                marker.Active = false;
                return base.OnMouseUp(state, args);
            }

            protected override bool OnClick(InputState state)
            {
                handleMouseInput(state);
                return true;
            }

            protected override bool OnDrag(InputState state)
            {
                handleMouseInput(state);
                return true;
            }

            private void handleMouseInput(InputState state)
            {
                // copied from SliderBar so we can do custom spacing logic.
                var xPosition = (ToLocalSpace(state?.Mouse.NativeState.Position ?? Vector2.Zero).X - RangePadding) / UsableWidth;

                CurrentNumber.Value = beatDivisor.ValidDivisors.OrderBy(d => Math.Abs(getMappedPosition(d) - xPosition)).First();
                OnUserChange();
            }

            private float getMappedPosition(float divisor) => (float)Math.Pow((divisor - 1) / (beatDivisor.ValidDivisors.Last() - 1), 0.90f);

            private class Tick : CompositeDrawable
            {
                private readonly int divisor;

                public Tick(int d)
                {
                    divisor = d;
                    Size = new Vector2(2.5f, 10);

                    InternalChild = new Box { RelativeSizeAxes = Axes.Both };

                    CornerRadius = 0.5f;
                    Masking = true;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Colour = getColourForDivisor(divisor, colours);
                }

                private ColourInfo getColourForDivisor(int divisor, OsuColour colours)
                {
                    switch (divisor)
                    {
                        case 2:
                            return colours.BlueLight;
                        case 4:
                            return colours.Blue;
                        case 8:
                            return colours.BlueDarker;
                        case 16:
                            return colours.PurpleDark;
                        case 32:
                            return colours.Purple;
                        case 3:
                            return colours.YellowLight;
                        case 6:
                            return colours.Yellow;
                        case 9:
                            return colours.YellowDark;
                        case 12:
                            return colours.YellowDarker;
                        case 18:
                            return colours.RedDarker;
                        case 24:
                            return colours.RedDark;
                        case 5:
                            return colours.GrayD;
                        case 7:
                            return colours.GrayB;
                        case 11:
                            return colours.Gray8;
                        default:
                            return Color4.White;
                    }
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
                            Blending = BlendingMode.Additive,
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
