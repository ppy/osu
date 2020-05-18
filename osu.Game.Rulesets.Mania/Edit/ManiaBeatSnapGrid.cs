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
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaBeatSnapGrid : Component
    {
        private const double visible_range = 750;

        [Resolved]
        private IManiaHitObjectComposer composer { get; set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> working { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        private readonly List<ScrollingHitObjectContainer> grids = new List<ScrollingHitObjectContainer>();

        private readonly Cached lineCache = new Cached();

        private (double start, double end)? selectionTimeRange;

        public (double start, double end)? SelectionTimeRange
        {
            get => selectionTimeRange;
            set
            {
                if (value == selectionTimeRange)
                    return;

                selectionTimeRange = value;
                lineCache.Invalidate();
            }
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

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var stage in composer.Playfield.Stages)
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
                if (nextTimingPoint != timingPoint)
                {
                    beat = 0;
                    timingPoint = nextTimingPoint;
                }

                Color4 colour = BindableBeatDivisor.GetColourFor(
                    BindableBeatDivisor.GetDivisorForBeatIndex(Math.Max(1, beat), beatDivisor.Value), colours);

                foreach (var grid in grids)
                    grid.Add(new DrawableGridLine(time, colour));

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

        public (Vector2 position, double time)? GetSnappedPosition(Vector2 position)
        {
            float minDist = float.PositiveInfinity;
            DrawableGridLine minDistLine = null;

            Vector2 minDistLinePosition = Vector2.Zero;

            foreach (var grid in grids)
            {
                foreach (var line in grid.AliveObjects.OfType<DrawableGridLine>())
                {
                    Vector2 linePos = line.ToSpaceOfOtherDrawable(line.OriginPosition, this);
                    float d = Vector2.Distance(position, linePos);

                    if (d < minDist)
                    {
                        minDist = d;
                        minDistLine = line;
                        minDistLinePosition = linePos;
                    }
                }
            }

            if (minDistLine == null)
                return null;

            float noteOffset = (scrollingInfo.Direction.Value == ScrollingDirection.Up ? 1 : -1) * DefaultNotePiece.NOTE_HEIGHT / 2;
            return (new Vector2(position.X, minDistLinePosition.Y + noteOffset), minDistLine.HitObject.StartTime);
        }

        private class DrawableGridLine : DrawableHitObject
        {
            [Resolved]
            private IScrollingInfo scrollingInfo { get; set; }

            private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

            public DrawableGridLine(double startTime, Color4 colour)
                : base(new HitObject { StartTime = startTime })
            {
                RelativeSizeAxes = Axes.X;
                Height = 2;

                AddInternal(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour
                });
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

            protected override void UpdateStateTransforms(ArmedState state)
            {
                using (BeginAbsoluteSequence(HitObject.StartTime + 1000))
                    this.FadeOut();
            }
        }
    }
}
