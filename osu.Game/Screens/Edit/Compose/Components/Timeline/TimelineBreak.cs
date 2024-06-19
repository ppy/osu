// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineBreak : CompositeDrawable
    {
        public BreakPeriod Break { get; }

        public TimelineBreak(BreakPeriod b)
        {
            Break = b;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.TopLeft;
            Padding = new MarginPadding { Horizontal = -5 };

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 5 },
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.PurpleLight,
                        Alpha = 0.4f,
                    },
                },
                new DragHandle(Break, isStartHandle: true)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Action = (time, breakPeriod) => breakPeriod.StartTime = time,
                },
                new DragHandle(Break, isStartHandle: false)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Action = (time, breakPeriod) => breakPeriod.EndTime = time,
                },
            };
        }

        protected override void Update()
        {
            base.Update();

            X = (float)Break.StartTime;
            Width = (float)Break.Duration;
        }

        private partial class DragHandle : FillFlowContainer
        {
            public new Anchor Anchor
            {
                get => base.Anchor;
                init => base.Anchor = value;
            }

            public Action<double, BreakPeriod>? Action { get; init; }

            private readonly BreakPeriod breakPeriod;
            private readonly bool isStartHandle;

            private Container handle = null!;
            private (double min, double max)? allowedDragRange;

            [Resolved]
            private EditorBeatmap beatmap { get; set; } = null!;

            [Resolved]
            private Timeline timeline { get; set; } = null!;

            [Resolved]
            private IEditorChangeHandler? changeHandler { get; set; }

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public DragHandle(BreakPeriod breakPeriod, bool isStartHandle)
            {
                this.breakPeriod = breakPeriod;
                this.isStartHandle = isStartHandle;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(5);

                Children = new Drawable[]
                {
                    handle = new Container
                    {
                        Anchor = Anchor,
                        Origin = Anchor,
                        RelativeSizeAxes = Axes.Y,
                        CornerRadius = 5,
                        Masking = true,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.White,
                        },
                    },
                    new OsuSpriteText
                    {
                        BypassAutoSizeAxes = Axes.X,
                        Anchor = Anchor,
                        Origin = Anchor,
                        Text = "Break",
                        Margin = new MarginPadding { Top = 2, },
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
                FinishTransforms(true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                changeHandler?.BeginChange();
                updateState();

                double min = beatmap.HitObjects.Last(ho => ho.GetEndTime() <= breakPeriod.StartTime).GetEndTime();
                double max = beatmap.HitObjects.First(ho => ho.StartTime >= breakPeriod.EndTime).StartTime;

                if (isStartHandle)
                    max = Math.Min(max, breakPeriod.EndTime - BreakPeriod.MIN_BREAK_DURATION);
                else
                    min = Math.Max(min, breakPeriod.StartTime + BreakPeriod.MIN_BREAK_DURATION);

                allowedDragRange = (min, max);

                return true;
            }

            protected override void OnDrag(DragEvent e)
            {
                base.OnDrag(e);

                Debug.Assert(allowedDragRange != null);

                if (timeline.FindSnappedPositionAndTime(e.ScreenSpaceMousePosition).Time is double time
                    && time > allowedDragRange.Value.min
                    && time < allowedDragRange.Value.max)
                {
                    Action?.Invoke(time, breakPeriod);
                }

                updateState();
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                changeHandler?.EndChange();
                updateState();
                base.OnDragEnd(e);
            }

            private void updateState()
            {
                bool active = IsHovered || IsDragged;

                var colour = colours.PurpleLighter;
                if (active)
                    colour = colour.Lighten(0.3f);

                this.FadeColour(colour, 400, Easing.OutQuint);
                handle.ResizeWidthTo(active ? 20 : 10, 400, Easing.OutElasticHalf);
            }
        }
    }
}
