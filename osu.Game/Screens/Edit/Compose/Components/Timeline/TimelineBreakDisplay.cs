// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Game.Beatmaps.Timing;
using osu.Game.Configuration;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineBreakDisplay : TimelinePart<TimelineBreak>
    {
        [Resolved]
        private Timeline timeline { get; set; } = null!;

        [Resolved]
        private IEditorChangeHandler? editorChangeHandler { get; set; }

        /// <summary>
        /// The visible time/position range of the timeline.
        /// </summary>
        private (float min, float max) visibleRange = (float.MinValue, float.MaxValue);

        private readonly Cached breakCache = new Cached();

        private readonly BindableList<BreakPeriod> breaks = new BindableList<BreakPeriod>();

        private readonly BindableBool showBreaks = new BindableBool(true);

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager)
        {
            configManager.BindWith(OsuSetting.EditorTimelineShowBreaks, showBreaks);
            showBreaks.BindValueChanged(_ => breakCache.Invalidate());
        }

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            breaks.UnbindAll();
            breaks.BindTo(beatmap.Breaks);
            breaks.BindCollectionChanged((_, e) =>
            {
                if (e.Action != NotifyCollectionChangedAction.Replace)
                    breakCache.Invalidate();
            });
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
            Clear();

            if (!showBreaks.Value)
                return;

            for (int i = 0; i < breaks.Count; i++)
            {
                var breakPeriod = breaks[i];

                if (!shouldBeVisible(breakPeriod))
                    continue;

                Add(new TimelineBreak(breakPeriod)
                {
                    OnDeleted = b =>
                    {
                        editorChangeHandler?.BeginChange();
                        breaks.Remove(b);
                        editorChangeHandler?.EndChange();
                    },
                });
            }
        }

        private bool shouldBeVisible(BreakPeriod breakPeriod) => breakPeriod.EndTime >= visibleRange.min && breakPeriod.StartTime <= visibleRange.max;
    }
}
