// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class BeatDivisorControl : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        private int? lastCustomDivisor;

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Name = "Main background",
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
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
                                        Name = "Tick area background",
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background5,
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
                                        Colour = colourProvider.Background3
                                    },
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                new ChevronButton
                                                {
                                                    Icon = FontAwesome.Solid.ChevronLeft,
                                                    Action = beatDivisor.SelectPrevious
                                                },
                                                new DivisorDisplay { BeatDivisor = { BindTarget = beatDivisor } },
                                                new ChevronButton
                                                {
                                                    Icon = FontAwesome.Solid.ChevronRight,
                                                    Action = beatDivisor.SelectNext
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
                        },
                        new Drawable[]
                        {
                            new GridContainer
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
                                        new DivisorTypeText { BeatDivisor = { BindTarget = beatDivisor } },
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
                        },
                        new Drawable[]
                        {
                            new TextFlowContainer(s => s.Font = s.Font.With(size: 14))
                            {
                                Padding = new MarginPadding { Horizontal = 15, Vertical = 8 },
                                Text = "beat snap",
                                RelativeSizeAxes = Axes.X,
                                TextAnchor = Anchor.TopCentre,
                            },
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatDivisor.ValidDivisors.BindValueChanged(valid =>
            {
                if (valid.NewValue.Type == BeatDivisorType.Custom)
                    lastCustomDivisor = valid.NewValue.Presets.Last();
            }, true);
        }

        private void cycleDivisorType(int direction)
        {
            int totalTypes = Enum.GetValues<BeatDivisorType>().Length;
            BeatDivisorType currentType = beatDivisor.ValidDivisors.Value.Type;

            Debug.Assert(Math.Abs(direction) == 1);

            cycleOnce();

            if (lastCustomDivisor == null && currentType == BeatDivisorType.Custom)
                cycleOnce();

            switch (currentType)
            {
                case BeatDivisorType.Common:
                    beatDivisor.SetArbitraryDivisor(4, true);
                    break;

                case BeatDivisorType.Triplets:
                    beatDivisor.SetArbitraryDivisor(6, true);
                    break;

                case BeatDivisorType.Custom:
                    Debug.Assert(lastCustomDivisor != null);
                    beatDivisor.SetArbitraryDivisor(lastCustomDivisor.Value);
                    break;
            }

            void cycleOnce() => currentType = (BeatDivisorType)(((int)currentType + totalTypes + direction) % totalTypes);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ShiftPressed && e.Key >= Key.Number1 && e.Key <= Key.Number9)
            {
                beatDivisor.SetArbitraryDivisor(e.Key - Key.Number0);
                return true;
            }

            return base.OnKeyDown(e);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorCycleNextBeatSnapDivisor:
                    beatDivisor.SelectNext();
                    return true;

                case GlobalAction.EditorCyclePreviousBeatSnapDivisor:
                    beatDivisor.SelectPrevious();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        internal partial class DivisorDisplay : OsuAnimatedButton, IHasPopover
        {
            public BindableBeatDivisor BeatDivisor { get; } = new BindableBeatDivisor();

            private readonly OsuSpriteText divisorText;

            public DivisorDisplay()
                : base(HoverSampleSet.Default)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                AutoSizeAxes = Axes.Both;

                Add(divisorText = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(size: 20),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding
                    {
                        Horizontal = 5
                    }
                });

                Action = this.ShowPopover;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                divisorText.Colour = colours.BlueLighter;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                updateState();
            }

            private void updateState()
            {
                BeatDivisor.BindValueChanged(val => divisorText.Text = $"1/{val.NewValue}", true);
            }

            public Popover GetPopover() => new CustomDivisorPopover
            {
                BeatDivisor = { BindTarget = BeatDivisor }
            };
        }

        internal partial class CustomDivisorPopover : OsuPopover
        {
            public BindableBeatDivisor BeatDivisor { get; } = new BindableBeatDivisor();

            private readonly OsuNumberBox divisorTextBox;

            public CustomDivisorPopover()
            {
                Child = new FillFlowContainer
                {
                    Width = 150,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        divisorTextBox = new AutoSelectTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            PlaceholderText = "Beat divisor"
                        },
                        new OsuTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = "Related divisors will be added to the list of presets."
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                BeatDivisor.BindValueChanged(_ => updateState(), true);
                divisorTextBox.OnCommit += (_, _) => setPresetsFromTextBoxEntry();
            }

            private void setPresetsFromTextBoxEntry()
            {
                if (!int.TryParse(divisorTextBox.Text, out int divisor) || divisor < 1 || divisor > 64)
                {
                    updateState();
                    return;
                }

                BeatDivisor.SetArbitraryDivisor(divisor);

                this.HidePopover();
            }

            private void updateState()
            {
                divisorTextBox.Text = BeatDivisor.Value.ToString();
            }
        }

        private partial class DivisorTypeText : OsuSpriteText
        {
            public BindableBeatDivisor BeatDivisor { get; } = new BindableBeatDivisor();

            public DivisorTypeText()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Font = OsuFont.Default.With(size: 14);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                BeatDivisor.ValidDivisors.BindValueChanged(val => Text = val.NewValue.Type.Humanize(LetterCasing.LowerCase), true);
            }
        }

        internal partial class ChevronButton : IconButton
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

        private partial class TickSliderBar : SliderBar<int>
        {
            private Marker marker = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            private readonly BindableBeatDivisor beatDivisor;

            public TickSliderBar(BindableBeatDivisor beatDivisor)
            {
                CurrentNumber.BindTo(this.beatDivisor = beatDivisor);

                RangePadding = 5;
                Padding = new MarginPadding { Horizontal = RangePadding };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                beatDivisor.ValidDivisors.BindValueChanged(_ => updateDivisors(), true);
            }

            private void updateDivisors()
            {
                ClearInternal();
                CurrentNumber.ValueChanged -= moveMarker;

                int largestDivisor = beatDivisor.ValidDivisors.Value.Presets.Last();

                for (int tickIndex = 0; tickIndex <= largestDivisor; tickIndex++)
                {
                    int divisor = BindableBeatDivisor.GetDivisorForBeatIndex(tickIndex, largestDivisor, (int[])beatDivisor.ValidDivisors.Value.Presets);
                    bool isSolidTick = divisor * (largestDivisor - tickIndex) == largestDivisor;

                    AddInternal(new Tick(divisor, isSolidTick)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.Centre,
                        RelativePositionAxes = Axes.Both,
                        Colour = BindableBeatDivisor.GetColourFor(divisor, colours),
                        X = tickIndex / (float)largestDivisor,
                    });
                }

                AddInternal(marker = new Marker());
                CurrentNumber.ValueChanged += moveMarker;
                CurrentNumber.TriggerChange();
            }

            private void moveMarker(ValueChangedEvent<int> divisor)
            {
                marker.MoveToX(getMappedPosition(divisor.NewValue), 100, Easing.OutQuint);

                foreach (Tick tick in InternalChildren.OfType<Tick>().Where(t => !t.AlwaysDisplayed))
                {
                    tick.FadeTo(divisor.NewValue % tick.Divisor == 0 ? 0.2f : 0f, 100, Easing.OutQuint);
                }
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
                        beatDivisor.SelectNext();
                        OnUserChange(Current.Value);
                        return true;

                    case Key.Left:
                        beatDivisor.SelectPrevious();
                        OnUserChange(Current.Value);
                        return true;

                    default:
                        return false;
                }
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                marker.Active = true;
                handleMouseInput(e.ScreenSpaceMousePosition);
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

                CurrentNumber.Value = beatDivisor.ValidDivisors.Value.Presets.MinBy(d => Math.Abs(getMappedPosition(d) - xPosition));
                OnUserChange(Current.Value);
            }

            private float getMappedPosition(float divisor) => 1 - 1 / divisor;

            private partial class Tick : Circle
            {
                public readonly bool AlwaysDisplayed;

                public readonly int Divisor;

                public Tick(int divisor, bool alwaysDisplayed)
                {
                    AlwaysDisplayed = alwaysDisplayed;
                    Divisor = divisor;

                    Size = new Vector2(6f, 12) * BindableBeatDivisor.GetSize(divisor);
                    Alpha = alwaysDisplayed ? 1 : 0;

                    InternalChild = new Box { RelativeSizeAxes = Axes.Both };
                }
            }

            private partial class Marker : CompositeDrawable
            {
                [Resolved]
                private OverlayColourProvider colourProvider { get; set; } = null!;

                [BackgroundDependencyLoader]
                private void load()
                {
                    Colour = colourProvider.Background3;
                    Anchor = Anchor.BottomLeft;
                    Origin = Anchor.BottomCentre;

                    Size = new Vector2(8, 6.5f);

                    RelativePositionAxes = Axes.X;

                    InternalChildren = new Drawable[]
                    {
                        new Triangle
                        {
                            RelativeSizeAxes = Axes.Both,
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
                        this.FadeColour(value ? colourProvider.Background1 : colourProvider.Background3, 500, Easing.OutQuint);
                        active = value;
                    }
                }
            }
        }

        private partial class AutoSelectTextBox : OsuNumberBox
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();

                GetContainingInputManager().ChangeFocus(this);
                SelectAll();
            }
        }
    }
}
