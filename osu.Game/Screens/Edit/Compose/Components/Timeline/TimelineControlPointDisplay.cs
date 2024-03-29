﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    /// <summary>
    /// The part of the timeline that displays the control points.
    /// </summary>
    public partial class TimelineControlPointDisplay : TimelinePart<TimelineControlPointGroup>
    {
        [Resolved]
        private Timeline timeline { get; set; } = null!;

        /// <summary>
        /// The visible time/position range of the timeline.
        /// </summary>
        private (float min, float max) visibleRange = (float.MinValue, float.MaxValue);

        private readonly Cached groupCache = new Cached();

        private readonly IBindableList<ControlPointGroup> controlPointGroups = new BindableList<ControlPointGroup>();

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            controlPointGroups.UnbindAll();
            controlPointGroups.BindTo(beatmap.ControlPointInfo.Groups);
            controlPointGroups.BindCollectionChanged((_, _) => groupCache.Invalidate(), true);
        }

        protected override void Update()
        {
            base.Update();

            if (DrawWidth <= 0) return;

            (float, float) newRange = (
                (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopLeft).X - TopPointPiece.WIDTH) / DrawWidth * Content.RelativeChildSize.X,
                (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopRight).X) / DrawWidth * Content.RelativeChildSize.X);

            if (visibleRange != newRange)
            {
                visibleRange = newRange;
                groupCache.Invalidate();
            }

            if (!groupCache.IsValid)
            {
                recreateDrawableGroups();
                groupCache.Validate();
            }
        }

        private void recreateDrawableGroups()
        {
            // Remove groups outside the visible range
            foreach (TimelineControlPointGroup drawableGroup in this)
            {
                if (!shouldBeVisible(drawableGroup.Group))
                    drawableGroup.Expire();
            }

            // Add remaining ones
            for (int i = 0; i < controlPointGroups.Count; i++)
            {
                var group = controlPointGroups[i];

                if (!shouldBeVisible(group))
                    continue;

                bool alreadyVisible = false;

                foreach (var g in this)
                {
                    if (ReferenceEquals(g.Group, group))
                    {
                        alreadyVisible = true;
                        break;
                    }
                }

                if (alreadyVisible)
                    continue;

                Add(new TimelineControlPointGroup(group));
            }
        }

        private bool shouldBeVisible(ControlPointGroup group) => group.Time >= visibleRange.min && group.Time <= visibleRange.max;
    }
}
