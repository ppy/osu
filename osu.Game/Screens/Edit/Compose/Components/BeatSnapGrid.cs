// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A grid which displays coloured beat divisor lines in proximity to the selection or placement cursor.
    /// </summary>
    public abstract partial class BeatSnapGrid : CompositeComponent
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
        private EditorBeatmap beatmap { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; } = null!;

        private readonly List<ScrollingHitObjectContainer> grids = new List<ScrollingHitObjectContainer>();

        private readonly DrawablePool<DrawableGridLine> linesPool = new DrawablePool<DrawableGridLine>(50);

        private readonly Cached lineCache = new Cached();

        private (double start, double end)? selectionTimeRange;

        [BackgroundDependencyLoader]
        private void load(HitObjectComposer composer)
        {
            AddInternal(linesPool);

            foreach (var target in GetTargetContainers(composer))
            {
                var lineContainer = new ScrollingHitObjectContainer();

                grids.Add(lineContainer);
                target.Add(lineContainer);
            }

            beatDivisor.BindValueChanged(_ => createLines(), true);
        }

        protected abstract IEnumerable<Container> GetTargetContainers(HitObjectComposer composer);

        protected override void Update()
        {
            base.Update();

            if (!lineCache.IsValid)
            {
                lineCache.Validate();
                createLines();
            }
        }

        private void createLines()
        {
            foreach (var grid in grids)
                grid.Clear();

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
                    BindableBeatDivisor.GetDivisorForBeatIndex(beat, beatDivisor.Value), colours);

                foreach (var grid in grids)
                {
                    var line = linesPool.Get();

                    line.Apply(new HitObject
                    {
                        StartTime = time
                    });

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

        private partial class DrawableGridLine : DrawableHitObject
        {
            [Resolved]
            private IScrollingInfo scrollingInfo { get; set; } = null!;

            private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

            public DrawableGridLine()
                : base(new HitObject())
            {
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

                bool isHorizontal = direction.NewValue == ScrollingDirection.Left || direction.NewValue == ScrollingDirection.Right;

                if (isHorizontal)
                {
                    RelativeSizeAxes = Axes.Y;
                    Width = 2;
                }
                else
                {
                    RelativeSizeAxes = Axes.X;
                    Height = 2;
                }
            }

            protected override void UpdateInitialTransforms()
            {
                // don't perform any fading – we are handling that ourselves.
                LifetimeEnd = HitObject.StartTime + visible_range;
            }
        }
    }
}
