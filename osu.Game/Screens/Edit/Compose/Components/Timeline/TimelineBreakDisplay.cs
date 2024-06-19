// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Game.Beatmaps.Timing;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineBreakDisplay : TimelinePart<TimelineBreak>
    {
        [Resolved]
        private Timeline timeline { get; set; } = null!;

        /// <summary>
        /// The visible time/position range of the timeline.
        /// </summary>
        private (float min, float max) visibleRange = (float.MinValue, float.MaxValue);

        private readonly Cached breakCache = new Cached();

        private readonly BindableList<BreakPeriod> breaks = new BindableList<BreakPeriod>();

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            // TODO: this will have to be mutable soon enough
            breaks.AddRange(beatmap.Breaks);
        }

        protected override void Update()
        {
            base.Update();

            if (DrawWidth <= 0) return;

            (float, float) newRange = (
                (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopLeft).X) / DrawWidth * Content.RelativeChildSize.X,
                (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopRight).X) / DrawWidth * Content.RelativeChildSize.X);

            if (visibleRange != newRange)
            {
                visibleRange = newRange;
                breakCache.Invalidate();
            }

            if (!breakCache.IsValid)
            {
                recreateBreaks();
                breakCache.Validate();
            }
        }

        private void recreateBreaks()
        {
            // Remove groups outside the visible range
            foreach (TimelineBreak drawableBreak in this)
            {
                if (!shouldBeVisible(drawableBreak.Break))
                    drawableBreak.Expire();
            }

            // Add remaining ones
            for (int i = 0; i < breaks.Count; i++)
            {
                var breakPeriod = breaks[i];

                if (!shouldBeVisible(breakPeriod))
                    continue;

                bool alreadyVisible = false;

                foreach (var b in this)
                {
                    if (ReferenceEquals(b.Break, breakPeriod))
                    {
                        alreadyVisible = true;
                        break;
                    }
                }

                if (alreadyVisible)
                    continue;

                Add(new TimelineBreak(breakPeriod));
            }
        }

        private bool shouldBeVisible(BreakPeriod breakPeriod) => breakPeriod.EndTime >= visibleRange.min && breakPeriod.StartTime <= visibleRange.max;
    }
}
