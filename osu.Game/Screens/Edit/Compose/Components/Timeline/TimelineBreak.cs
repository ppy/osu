// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineBreak : CompositeDrawable, IHasContextMenu
    {
        public Bindable<BreakPeriod> Break { get; } = new Bindable<BreakPeriod>();

        public Action<BreakPeriod>? OnDeleted { get; init; }

        public TimelineBreak(BreakPeriod b)
        {
            Break.Value = b;
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
                        Colour = colours.Gray5,
                        Alpha = 0.9f,
                    },
                },
                new DragHandle(isStartHandle: true)
                {
                    Break = { BindTarget = Break },
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Action = (time, breakPeriod) => new ManualBreakPeriod(time, breakPeriod.EndTime),
                },
                new DragHandle(isStartHandle: false)
                {
                    Break = { BindTarget = Break },
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Action = (time, breakPeriod) => new ManualBreakPeriod(breakPeriod.StartTime, time),
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Break.BindValueChanged(_ =>
            {
                X = (float)Break.Value.StartTime;
                Width = (float)Break.Value.Duration;
            }, true);
        }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem(CommonStrings.ButtonsDelete, MenuItemType.Destructive, () => OnDeleted?.Invoke(Break.Value)),
        };

        private partial class DragHandle : FillFlowContainer
        {
            public Bindable<BreakPeriod> Break { get; } = new Bindable<BreakPeriod>();

            public new Anchor Anchor
            {
                get => base.Anchor;
                init => base.Anchor = value;
            }

            public Func<double, BreakPeriod, BreakPeriod>? Action { get; init; }

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

            public DragHandle(bool isStartHandle)
            {
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
                        CornerRadius = 4,
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

                double min = beatmap.HitObjects.LastOrDefault(ho => ho.GetEndTime() <= Break.Value.StartTime)?.GetEndTime() ?? double.NegativeInfinity;
                double max = beatmap.HitObjects.FirstOrDefault(ho => ho.StartTime >= Break.Value.EndTime)?.StartTime ?? double.PositiveInfinity;

                if (isStartHandle)
                    max = Math.Min(max, Break.Value.EndTime - BreakPeriod.MIN_BREAK_DURATION);
                else
                    min = Math.Max(min, Break.Value.StartTime + BreakPeriod.MIN_BREAK_DURATION);

                allowedDragRange = (min, max);

                return true;
            }

            protected override void OnDrag(DragEvent e)
            {
                base.OnDrag(e);

                Debug.Assert(allowedDragRange != null);

                if (Action != null
                    && timeline.FindSnappedPositionAndTime(e.ScreenSpaceMousePosition).Time is double time
                    && time > allowedDragRange.Value.min
                    && time < allowedDragRange.Value.max)
                {
                    int index = beatmap.Breaks.IndexOf(Break.Value);
                    beatmap.Breaks[index] = Break.Value = Action.Invoke(time, Break.Value);
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

                var colour = colours.Gray8;
                if (active)
                    colour = colour.Lighten(0.3f);

                handle.FadeColour(colour, 400, Easing.OutQuint);
                handle.ResizeWidthTo(active ? 10 : 8, 400, Easing.OutElasticHalf);
            }
        }
    }
}
