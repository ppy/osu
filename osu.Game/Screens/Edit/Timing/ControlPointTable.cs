// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Timing.RowAttributes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class ControlPointTable : CompositeDrawable
    {
        public BindableList<ControlPointGroup> Groups { get; } = new BindableList<ControlPointGroup>();

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        [Cached]
        private Bindable<TimingControlPoint?> activeTimingPoint { get; } = new Bindable<TimingControlPoint?>();

        [Cached]
        private Bindable<EffectControlPoint?> activeEffectPoint { get; } = new Bindable<EffectControlPoint?>();

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        [Resolved]
        private Bindable<ControlPointGroup?> selectedGroup { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        private const float timing_column_width = 300;
        private const float row_height = 25;
        private const float row_horizontal_padding = 20;

        private ControlPointRowList list = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Background4,
                    RelativeSizeAxes = Axes.Both,
                },
                new Box
                {
                    Colour = colours.Background3,
                    RelativeSizeAxes = Axes.Y,
                    Width = timing_column_width + 10,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = row_height,
                    Padding = new MarginPadding { Horizontal = row_horizontal_padding },
                    Children = new Drawable[]
                    {
                        new TableHeaderText("Time")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new TableHeaderText("Attributes")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = timing_column_width }
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = row_height },
                    Child = list = new ControlPointRowList
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowData = { BindTarget = Groups, },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedGroup.BindValueChanged(_ => scrollToMostRelevantRow(force: true), true);
        }

        protected override void Update()
        {
            base.Update();

            scrollToMostRelevantRow(force: false);
        }

        private void scrollToMostRelevantRow(bool force)
        {
            double accurateTime = editorClock.CurrentTimeAccurate;

            activeTimingPoint.Value = beatmap.ControlPointInfo.TimingPointAt(accurateTime);
            activeEffectPoint.Value = beatmap.ControlPointInfo.EffectPointAt(accurateTime);

            double latestActiveTime = Math.Max(activeTimingPoint.Value?.Time ?? double.NegativeInfinity, activeEffectPoint.Value?.Time ?? double.NegativeInfinity);
            var groupToShow = selectedGroup.Value ?? beatmap.ControlPointInfo.GroupAt(latestActiveTime);
            list.ScrollTo(groupToShow, force);
        }

        private partial class ControlPointRowList : VirtualisedListContainer<ControlPointGroup, DrawableControlGroup>
        {
            public ControlPointRowList()
                : base(row_height, 50)
            {
            }

            protected override ScrollContainer<Drawable> CreateScrollContainer() => new UserTrackingScrollContainer();

            protected new UserTrackingScrollContainer Scroll => (UserTrackingScrollContainer)base.Scroll;

            public void ScrollTo(ControlPointGroup group, bool force)
            {
                if (Scroll.UserScrolling && !force)
                    return;

                // can't use `.ScrollIntoView()` here because of the list virtualisation not giving
                // child items valid coordinates from the start, so ballpark something similar
                // using estimated row height.
                var row = Items.FlowingChildren.SingleOrDefault(item => item.Row.Equals(group));

                if (row == null)
                    return;

                float minPos = row.Y;
                float maxPos = minPos + row_height;

                if (minPos < Scroll.Current)
                    Scroll.ScrollTo(minPos);
                else if (maxPos > Scroll.Current + Scroll.DisplayableContent)
                    Scroll.ScrollTo(maxPos - Scroll.DisplayableContent);
            }
        }

        public partial class DrawableControlGroup : PoolableDrawable, IHasCurrentValue<ControlPointGroup>
        {
            public Bindable<ControlPointGroup> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            private readonly BindableWithCurrent<ControlPointGroup> current = new BindableWithCurrent<ControlPointGroup>();

            private Box background = null!;
            private Drawable currentIndicator = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved]
            private Bindable<ControlPointGroup?> selectedGroup { get; set; } = null!;

            [Resolved]
            private Bindable<TimingControlPoint?> activeTimingPoint { get; set; } = null!;

            [Resolved]
            private Bindable<EffectControlPoint?> activeEffectPoint { get; set; } = null!;

            [Resolved]
            private EditorClock editorClock { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChildren = new[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background1,
                        Alpha = 0,
                    },
                    currentIndicator = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Y,
                                Width = 5,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Y,
                                Blending = BlendingParameters.Additive,
                                X = 5,
                                Width = 150,
                                Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0.1f), Color4.White.Opacity(0))
                            },
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = row_horizontal_padding, },
                        Children = new Drawable[]
                        {
                            new ControlGroupTiming { Group = { BindTarget = current }, },
                            new ControlGroupAttributes(point => point is not TimingControlPoint)
                            {
                                Group = { BindTarget = current },
                                Margin = new MarginPadding { Left = timing_column_width }
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                selectedGroup.BindValueChanged(_ => updateState());
                activeEffectPoint.BindValueChanged(_ => updateState());
                activeTimingPoint.BindValueChanged(_ => updateState(), true);
                FinishTransforms(true);
            }

            protected override void PrepareForUse()
            {
                base.PrepareForUse();

                updateState();
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                updateState();
            }

            protected override bool OnClick(ClickEvent e)
            {
                // schedule to give time for any modified focused text box to lose focus and commit changes (e.g. BPM / time signature textboxes) before switching to new point.
                var currentGroup = Current.Value;
                Schedule(() =>
                {
                    selectedGroup.Value = currentGroup;
                    editorClock.SeekSmoothlyTo(currentGroup.Time);
                });
                return true;
            }

            private void updateState()
            {
                bool isSelected = selectedGroup.Value?.Equals(current.Value) == true;

                bool hasCurrentTimingPoint = activeTimingPoint.Value != null && current.Value.ControlPoints.Contains(activeTimingPoint.Value);
                bool hasCurrentEffectPoint = activeEffectPoint.Value != null && current.Value.ControlPoints.Contains(activeEffectPoint.Value);

                background.FadeTo(IsHovered || isSelected ? 1 : 0, 100, Easing.OutQuint);
                background.FadeColour(isSelected ? colourProvider.Colour3 : colourProvider.Background1, 100, Easing.OutQuint);

                if (hasCurrentTimingPoint || hasCurrentEffectPoint)
                {
                    currentIndicator.FadeIn(100, Easing.OutQuint);

                    if (hasCurrentTimingPoint && hasCurrentEffectPoint)
                        currentIndicator.Colour = ColourInfo.GradientVertical(activeTimingPoint.Value!.GetRepresentingColour(colours), activeEffectPoint.Value!.GetRepresentingColour(colours));
                    else if (hasCurrentTimingPoint)
                        currentIndicator.Colour = activeTimingPoint.Value!.GetRepresentingColour(colours);
                    else
                        currentIndicator.Colour = activeEffectPoint.Value!.GetRepresentingColour(colours);
                }
                else
                    currentIndicator.FadeOut(100, Easing.OutQuint);
            }
        }

        private partial class ControlGroupTiming : FillFlowContainer
        {
            public Bindable<ControlPointGroup> Group { get; } = new Bindable<ControlPointGroup>();

            private OsuSpriteText timeText = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Name = @"ControlGroupTiming";
                RelativeSizeAxes = Axes.Y;
                Width = timing_column_width;
                Spacing = new Vector2(5);
                Children = new Drawable[]
                {
                    timeText = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                        Width = 70,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    new ControlGroupAttributes(c => c is TimingControlPoint)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Group = { BindTarget = Group },
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Group.BindValueChanged(_ => timeText.Text = Group.Value?.Time.ToEditorFormattedString() ?? default(LocalisableString), true);
            }
        }

        private partial class ControlGroupAttributes : CompositeDrawable
        {
            public Bindable<ControlPointGroup> Group { get; } = new Bindable<ControlPointGroup>();
            private BindableList<ControlPoint> controlPoints { get; } = new BindableList<ControlPoint>();

            private readonly Func<ControlPoint, bool> matchFunction;

            private FillFlowContainer fill = null!;

            public ControlGroupAttributes(Func<ControlPoint, bool> matchFunction)
            {
                this.matchFunction = matchFunction;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;
                Name = @"ControlGroupAttributes";

                InternalChild = fill = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(2)
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Group.BindValueChanged(_ =>
                {
                    controlPoints.UnbindBindings();
                    controlPoints.Clear();
                    if (Group.Value != null)
                        ((IBindableList<ControlPoint>)controlPoints).BindTo(Group.Value.ControlPoints);
                }, true);

                controlPoints.BindCollectionChanged((_, _) => createChildren(), true);
            }

            private void createChildren()
            {
                fill.ChildrenEnumerable = controlPoints
                                          .Where(matchFunction)
                                          .Select(createAttribute)
                                          // arbitrary ordering to make timing points first.
                                          // probably want to explicitly define order in the future.
                                          .OrderByDescending(c => c.GetType().Name);
            }

            private Drawable createAttribute(ControlPoint controlPoint)
            {
                switch (controlPoint)
                {
                    case TimingControlPoint timing:
                        return new TimingRowAttribute(timing);

                    case EffectControlPoint effect:
                        return new EffectRowAttribute(effect);
                }

                throw new ArgumentOutOfRangeException(nameof(controlPoint), $"Control point type {controlPoint.GetType()} is not supported");
            }
        }
    }
}
