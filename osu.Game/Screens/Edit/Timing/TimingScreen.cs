// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class TimingScreen : EditorScreenWithTimeline
    {
        [Cached]
        public readonly Bindable<ControlPointGroup> SelectedGroup = new Bindable<ControlPointGroup>();

        private readonly Bindable<EditorScreenMode> currentEditorMode = new Bindable<EditorScreenMode>();

        [Resolved]
        private EditorClock? editorClock { get; set; }

        public TimingScreen()
            : base(EditorScreenMode.Timing)
        {
        }

        protected override Drawable CreateMainContent() => new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 350),
            },
            Content = new[]
            {
                new Drawable[]
                {
                    new ControlPointList
                    {
                        SelectClosestTimingPoint = selectClosestTimingPoint,
                    },
                    new ControlPointSettings(),
                },
            }
        };

        [BackgroundDependencyLoader]
        private void load(Editor? editor)
        {
            if (editor != null)
                currentEditorMode.BindTo(editor.Mode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // When entering the timing screen, let's choose the closest valid timing point.
            // This will emulate the osu-stable behaviour where a metronome and timing information
            // are presented on entering the screen.
            currentEditorMode.BindValueChanged(mode =>
            {
                if (mode.NewValue == EditorScreenMode.Timing)
                    selectClosestTimingPoint();
            });
            selectClosestTimingPoint();
        }

        private void selectClosestTimingPoint()
        {
            if (editorClock == null)
                return;

            double accurateTime = editorClock.CurrentTimeAccurate;

            var activeTimingPoint = EditorBeatmap.ControlPointInfo.TimingPointAt(accurateTime);
            var activeEffectPoint = EditorBeatmap.ControlPointInfo.EffectPointAt(accurateTime);

            if (activeEffectPoint.Equals(EffectControlPoint.DEFAULT))
                SelectedGroup.Value = EditorBeatmap.ControlPointInfo.GroupAt(activeTimingPoint.Time);
            else
            {
                double latestActiveTime = Math.Max(activeTimingPoint.Time, activeEffectPoint.Time);
                SelectedGroup.Value = EditorBeatmap.ControlPointInfo.GroupAt(latestActiveTime);
            }
        }

        protected override void ConfigureTimeline(TimelineArea timelineArea)
        {
            base.ConfigureTimeline(timelineArea);

            timelineArea.Timeline.AlwaysShowControlPoints = true;
        }
    }
}
