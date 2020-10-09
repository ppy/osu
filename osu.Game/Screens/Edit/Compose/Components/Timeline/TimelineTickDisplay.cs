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
            beatDivisor.BindValueChanged(_ => tickCache.Invalidate());
        }

        private (float min, float max) visibleRange = (float.MinValue, float.MaxValue);

        [Resolved(canBeNull: true)]
        private Timeline timeline { get; set; }

        protected override void Update()
        {
            base.Update();

            if (timeline != null)
            {
                var newRange = (
                    ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopLeft).X / DrawWidth * Content.RelativeChildSize.X,
                    ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopRight).X / DrawWidth * Content.RelativeChildSize.X);

                if (visibleRange != newRange)
                    tickCache.Invalidate();

                visibleRange = newRange;
            }

            if (!tickCache.IsValid)
                createTicks();
        }

        private void createTicks()
        {
            int drawableIndex = 0;

            for (var i = 0; i < beatmap.ControlPointInfo.TimingPoints.Count; i++)
            {
                var point = beatmap.ControlPointInfo.TimingPoints[i];
                var until = i + 1 < beatmap.ControlPointInfo.TimingPoints.Count ? beatmap.ControlPointInfo.TimingPoints[i + 1].Time : working.Value.Track.Length;

                int beat = 0;

                for (double t = point.Time; t < until; t += point.BeatLength / beatDivisor.Value)
                {
                    if (t >= visibleRange.min && t <= visibleRange.max)
                    {
                        var indexInBeat = beat % beatDivisor.Value;

                        if (indexInBeat == 0)
                        {
                            var downbeatPoint = getNextUsablePoint();
                            downbeatPoint.X = (float)t;

                            downbeatPoint.Colour = BindableBeatDivisor.GetColourFor(1, colours);
                            downbeatPoint.Anchor = Anchor.TopLeft;
                            downbeatPoint.Origin = Anchor.TopCentre;
                            downbeatPoint.Height = 1;
                        }
                        else
                        {
                            var divisor = BindableBeatDivisor.GetDivisorForBeatIndex(beat, beatDivisor.Value);
                            var colour = BindableBeatDivisor.GetColourFor(divisor, colours);
                            var height = 0.1f - (float)divisor / BindableBeatDivisor.VALID_DIVISORS.Last() * 0.08f;

                            var topPoint = getNextUsablePoint();
                            topPoint.X = (float)t;
                            topPoint.Colour = colour;
                            topPoint.Height = height;
                            topPoint.Anchor = Anchor.TopLeft;
                            topPoint.Origin = Anchor.TopCentre;

                            var bottomPoint = getNextUsablePoint();
                            bottomPoint.X = (float)t;
                            bottomPoint.Colour = colour;
                            bottomPoint.Anchor = Anchor.BottomLeft;
                            bottomPoint.Origin = Anchor.BottomCentre;
                            bottomPoint.Height = height;
                        }
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
                    point = Children[drawableIndex++];

                point.Show();

                return point;
            }
        }
    }
}
