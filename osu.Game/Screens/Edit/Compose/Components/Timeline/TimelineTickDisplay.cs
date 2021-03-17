// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineTickDisplay : TimelinePart<PointVisualisation>
    {
        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> working { get; set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

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

        [Resolved(canBeNull: true)]
        private Timeline timeline { get; set; }

        protected override void Update()
        {
            base.Update();

            if (timeline != null)
            {
                var newRange = (
                    (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopLeft).X - PointVisualisation.WIDTH * 2) / DrawWidth * Content.RelativeChildSize.X,
                    (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopRight).X + PointVisualisation.WIDTH * 2) / DrawWidth * Content.RelativeChildSize.X);

                if (visibleRange != newRange)
                {
                    visibleRange = newRange;

                    // actual regeneration only needs to occur if we've passed one of the known next min/max tick boundaries.
                    if (nextMinTick == null || nextMaxTick == null || (visibleRange.min < nextMinTick || visibleRange.max > nextMaxTick))
                        tickCache.Invalidate();
                }
            }

            if (!tickCache.IsValid)
                createTicks();
        }

        private void createTicks()
        {
            int drawableIndex = 0;
            int highestDivisor = BindableBeatDivisor.VALID_DIVISORS.Last();

            nextMinTick = null;
            nextMaxTick = null;

            for (var i = 0; i < beatmap.ControlPointInfo.TimingPoints.Count; i++)
            {
                var point = beatmap.ControlPointInfo.TimingPoints[i];
                var until = i + 1 < beatmap.ControlPointInfo.TimingPoints.Count ? beatmap.ControlPointInfo.TimingPoints[i + 1].Time : working.Value.Track.Length;

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

                        var indexInBar = beat % ((int)point.TimeSignature * beatDivisor.Value);

                        var divisor = BindableBeatDivisor.GetDivisorForBeatIndex(beat, beatDivisor.Value);
                        var colour = BindableBeatDivisor.GetColourFor(divisor, colours);

                        // even though "bar lines" take up the full vertical space, we render them in two pieces because it allows for less anchor/origin churn.
                        var height = indexInBar == 0 ? 0.5f : 0.1f - (float)divisor / highestDivisor * 0.08f;

                        var topPoint = getNextUsablePoint();
                        topPoint.X = xPos;
                        topPoint.Colour = colour;
                        topPoint.Height = height;
                        topPoint.Anchor = Anchor.TopLeft;
                        topPoint.Origin = Anchor.TopCentre;

                        var bottomPoint = getNextUsablePoint();
                        bottomPoint.X = xPos;
                        bottomPoint.Colour = colour;
                        bottomPoint.Anchor = Anchor.BottomLeft;
                        bottomPoint.Origin = Anchor.BottomCentre;
                        bottomPoint.Height = height;
                    }

                    beat++;
                }
            }

            int usedDrawables = drawableIndex;

            // save a few drawables beyond the currently used for edge cases.
            while (drawableIndex < Math.Min(usedDrawables + 16, Count))
                Children[drawableIndex++].Hide();

            // expire any excess
            while (drawableIndex < Count)
                Children[drawableIndex++].Expire();

            tickCache.Validate();

            Drawable getNextUsablePoint()
            {
                PointVisualisation point;
                if (drawableIndex >= Count)
                    Add(point = new PointVisualisation());
                else
                    point = Children[drawableIndex];

                drawableIndex++;
                point.Show();

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
