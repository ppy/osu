// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineTickDisplay : TimelinePart<PointVisualisation>
    {
        // With current implementation every tick in the sub-tree should be visible, no need to check whether they are masked away.
        public override bool UpdateSubTreeMasking(Drawable source, RectangleF maskingBounds) => false;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        [Resolved]
        private Bindable<WorkingBeatmap> working { get; set; } = null!;

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; } = null!;

        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public TimelineTickDisplay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        private readonly Cached tickCache = new Cached();

        [BackgroundDependencyLoader]
        private void load()
        {
            beatDivisor.BindValueChanged(_ => invalidateTicks());

            if (changeHandler != null)
                // currently this is the best way to handle any kind of timing changes.
                changeHandler.OnStateChange += invalidateTicks;
        }

        private void invalidateTicks()
        {
            tickCache.Invalidate();
        }

        /// <summary>
        /// The visible time/position range of the timeline.
        /// </summary>
        private (float min, float max) visibleRange = (float.MinValue, float.MaxValue);

        /// <summary>
        /// The next time/position value to the left of the display when tick regeneration needs to be run.
        /// </summary>
        private float? nextMinTick;

        /// <summary>
        /// The next time/position value to the right of the display when tick regeneration needs to be run.
        /// </summary>
        private float? nextMaxTick;

        [Resolved]
        private Timeline? timeline { get; set; }

        protected override void Update()
        {
            base.Update();

            if (timeline == null || DrawWidth <= 0) return;

            (float, float) newRange = (
                (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopLeft).X - PointVisualisation.MAX_WIDTH * 2) / DrawWidth * Content.RelativeChildSize.X,
                (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopRight).X + PointVisualisation.MAX_WIDTH * 2) / DrawWidth * Content.RelativeChildSize.X);

            if (visibleRange != newRange)
            {
                visibleRange = newRange;

                // actual regeneration only needs to occur if we've passed one of the known next min/max tick boundaries.
                if (nextMinTick == null || nextMaxTick == null || (visibleRange.min < nextMinTick || visibleRange.max > nextMaxTick))
                    tickCache.Invalidate();
            }

            if (!tickCache.IsValid)
                createTicks();
        }

        private void createTicks()
        {
            int drawableIndex = 0;

            nextMinTick = null;
            nextMaxTick = null;

            for (int i = 0; i < beatmap.ControlPointInfo.TimingPoints.Count; i++)
            {
                var point = beatmap.ControlPointInfo.TimingPoints[i];
                double until = i + 1 < beatmap.ControlPointInfo.TimingPoints.Count ? beatmap.ControlPointInfo.TimingPoints[i + 1].Time : working.Value.Track.Length;

                int beat = 0;

                for (double t = point.Time; t < until; t += point.BeatLength / beatDivisor.Value)
                {
                    float xPos = (float)t;

                    if (t < visibleRange.min)
                        nextMinTick = xPos;
                    else if (t > visibleRange.max)
                        nextMaxTick ??= xPos;
                    else
                    {
                        // if this is the first beat in the beatmap, there is no next min tick
                        if (beat == 0 && i == 0)
                            nextMinTick = float.MinValue;

                        int indexInBar = beat % (point.TimeSignature.Numerator * beatDivisor.Value);

                        int divisor = BindableBeatDivisor.GetDivisorForBeatIndex(beat, beatDivisor.Value);
                        var colour = BindableBeatDivisor.GetColourFor(divisor, colours);

                        // even though "bar lines" take up the full vertical space, we render them in two pieces because it allows for less anchor/origin churn.

                        Vector2 size = Vector2.One;

                        if (indexInBar != 0)
                            size = BindableBeatDivisor.GetSize(divisor);

                        var line = getNextUsableLine();
                        line.X = xPos;
                        line.Width = PointVisualisation.MAX_WIDTH * size.X;
                        line.Height = 0.9f * size.Y;
                        line.Colour = colour;
                    }

                    beat++;
                }
            }

            if (Children.Count > 512)
            {
                // There should always be a sanely small number of ticks rendered.
                // If this assertion triggers, either the zoom logic is broken or a beatmap is
                // probably doing weird things...
                //
                // Let's hope the latter never happens.
                // If it does, we can choose to either fix it or ignore it as an outlier.
                string message = $"Timeline is rendering many ticks ({Children.Count})";

                Logger.Log(message);
                Debug.Fail(message);
            }

            int usedDrawables = drawableIndex;

            // save a few drawables beyond the currently used for edge cases.
            while (drawableIndex < Math.Min(usedDrawables + 16, Count))
                Children[drawableIndex++].Alpha = 0;

            // expire any excess
            while (drawableIndex < Count)
                Children[drawableIndex++].Expire();

            tickCache.Validate();

            Drawable getNextUsableLine()
            {
                PointVisualisation point;
                if (drawableIndex >= Count)
                    Add(point = new PointVisualisation());
                else
                    point = Children[drawableIndex];

                drawableIndex++;
                point.Alpha = 1;

                return point;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (changeHandler != null)
                changeHandler.OnStateChange -= invalidateTicks;
        }
    }
}
