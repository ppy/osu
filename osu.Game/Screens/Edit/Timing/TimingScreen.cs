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
    }
}
