// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class TimingScreen : EditorScreenWithTimeline
    {
        [Cached]
        public readonly Bindable<ControlPointGroup> SelectedGroup = new Bindable<ControlPointGroup>();

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
                    new ControlPointList(),
                    new ControlPointSettings(),
                },
            }
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (editorClock != null)
            {
                // When entering the timing screen, let's choose the closest valid timing point.
                // This will emulate the osu-stable behaviour where a metronome and timing information
                // are presented on entering the screen.
                var nearestTimingPoint = EditorBeatmap.ControlPointInfo.TimingPointAt(editorClock.CurrentTime);
                SelectedGroup.Value = EditorBeatmap.ControlPointInfo.GroupAt(nearestTimingPoint.Time);
            }
        }
    }
}
