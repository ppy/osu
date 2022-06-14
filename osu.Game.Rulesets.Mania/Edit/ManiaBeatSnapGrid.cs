// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Edit
{
    /// <summary>
    /// A grid which displays coloured beat divisor lines in proximity to the selection or placement cursor.
    /// </summary>
    public class ManiaBeatSnapGrid : Component
    {
        private const double visible_range = 750;

        /// <summary>
        /// The range of time values of the current selection.
        /// </summary>
        public (double start, double end)? SelectionTimeRange
        {
            set
            {
                if (value == selectionTimeRange)
                    return;

                selectionTimeRange = value;
                lineCache.Invalidate();
            }
        }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        private readonly List<ScrollingHitObjectContainer> grids = new List<ScrollingHitObjectContainer>();

        private readonly Cached lineCache = new Cached();

        private (double start, double end)? selectionTimeRange;

        [BackgroundDependencyLoader]
        private void load(HitObjectComposer composer)
        {
            foreach (var stage in ((ManiaPlayfield)composer.Playfield).Stages)
            {
                foreach (var column in stage.Columns)
                {
                    var lineContainer = new ScrollingHitObjectContainer();

                    grids.Add(lineContainer);
                    column.UnderlayElements.Add(lineContainer);
                }
            }

            beatDivisor.BindValueChanged(_ => createLines(), true);
        }

        protected override void Update()
        {
            base.Update();

            if (!lineCache.IsValid)
            {
                lineCache.Validate();
                createLines();
            }
        }

        private readonly Stack<DrawableGridLine> availableLines = new Stack<DrawableGridLine>();

        private void createLines()
        {
            foreach (var grid in grids)
            {
                foreach (var line in grid.Objects.OfType<DrawableGridLine>())
                    availableLines.Push(line);

                grid.Clear();
            }

            if (selectionTimeRange == null)
                return;

            var range = selectionTimeRange.Value;

            var timingPoint = beatmap.ControlPointInfo.TimingPointAt(range.start - visible_range);

            double time = timingPoint.Time;
            int beat = 0;

            // progress time until in the visible range.
            while (time < range.start - visible_range)
            {
                time += timingPoint.BeatLength / beatDivisor.Value;
                beat++;
            }

            while (time < range.end + visible_range)
            {
                var nextTimingPoint = beatmap.ControlPointInfo.TimingPointAt(time);

                // switch to the next timing point if we have reached it.
                if (nextTimingPoint.Time > timingPoint.Time)
                {
                    beat = 0;
                    time = nextTimingPoint.Time;
                    timingPoint = nextTimingPoint;
                }

                Color4 colour = BindableBeatDivisor.GetColourFor(
                    BindableBeatDivisor.GetDivisorForBeatIndex(Math.Max(1, beat), beatDivisor.Value), colours);

                foreach (var grid in grids)
                {
                    if (!availableLines.TryPop(out var line))
                        line = new DrawableGridLine();

                    line.HitObject.StartTime = time;
                    line.Colour = colour;

                    grid.Add(line);
                }

                beat++;
                time += timingPoint.BeatLength / beatDivisor.Value;
            }

            foreach (var grid in grids)
            {
                // required to update ScrollingHitObjectContainer's cache.
                grid.UpdateSubTree();

                foreach (var line in grid.Objects.OfType<DrawableGridLine>())
                {
                    time = line.HitObject.StartTime;

                    if (time >= range.start && time <= range.end)
                        line.Alpha = 1;
                    else
                    {
                        double timeSeparation = time < range.start ? range.start - time : time - range.end;
                        line.Alpha = (float)Math.Max(0, 1 - timeSeparation / visible_range);
                    }
                }
            }
        }

        private class DrawableGridLine : DrawableHitObject
        {
            [Resolved]
            private IScrollingInfo scrollingInfo { get; set; }

            private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

            public DrawableGridLine()
                : base(new HitObject())
            {
                RelativeSizeAxes = Axes.X;
                Height = 2;

                AddInternal(new Box { RelativeSizeAxes = Axes.Both });
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                direction.BindTo(scrollingInfo.Direction);
                direction.BindValueChanged(onDirectionChanged, true);
            }

            private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
            {
                Origin = Anchor = direction.NewValue == ScrollingDirection.Up
                    ? Anchor.TopLeft
                    : Anchor.BottomLeft;
            }

            protected override void UpdateInitialTransforms()
            {
                // don't perform any fading – we are handling that ourselves.
                LifetimeEnd = HitObject.StartTime + visible_range;
            }
        }
    }
}
