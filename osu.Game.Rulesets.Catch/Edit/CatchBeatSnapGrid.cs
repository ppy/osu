// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Edit
{
    /// <summary>
    /// A grid which displays coloured beat divisor lines in proximity to the selection or placement cursor.
    /// </summary>
    /// <remarks>
    /// This class heavily borrows from osu!mania's implementation (ManiaBeatSnapGrid).
    /// If further changes are to be made, they should also be applied there.
    /// If the scale of the changes are large enough, abstracting may be a good path.
    /// </remarks>
    public partial class CatchBeatSnapGrid : Component
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

        private readonly Cached lineCache = new Cached();

        private (double start, double end)? selectionTimeRange;

        private ScrollingHitObjectContainer lineContainer = null!;

        [BackgroundDependencyLoader]
        private void load(HitObjectComposer composer)
        {
            lineContainer = new ScrollingHitObjectContainer();

            ((CatchPlayfield)composer.Playfield).UnderlayElements.Add(lineContainer);

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
            foreach (var line in lineContainer.Objects.OfType<DrawableGridLine>())
                availableLines.Push(line);

            lineContainer.Clear();

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

                if (!availableLines.TryPop(out var line))
                    line = new DrawableGridLine();

                line.HitObject.StartTime = time;
                line.Colour = colour;

                lineContainer.Add(line);

                beat++;
                time += timingPoint.BeatLength / beatDivisor.Value;
            }

            // required to update ScrollingHitObjectContainer's cache.
            lineContainer.UpdateSubTree();

            foreach (var line in lineContainer.Objects.OfType<DrawableGridLine>())
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

        private partial class DrawableGridLine : DrawableHitObject
        {
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
                Origin = Anchor.BottomLeft;
                Anchor = Anchor.BottomLeft;
            }

            protected override void UpdateInitialTransforms()
            {
                // don't perform any fading – we are handling that ourselves.
                LifetimeEnd = HitObject.StartTime + visible_range;
            }
        }
    }
}
