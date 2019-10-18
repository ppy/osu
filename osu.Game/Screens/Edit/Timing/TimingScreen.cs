// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Edit.Timing
{
    public class TimingScreen : EditorScreenWithTimeline
    {
        [Cached]
        private readonly Bindable<ControlPoint> controlPoint = new Bindable<ControlPoint>();

        protected override Drawable CreateMainContent() => new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 200),
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

        public class ControlPointList : CompositeDrawable
        {
            [Resolved]
            protected IBindable<WorkingBeatmap> Beatmap { get; private set; }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.Gray0,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new ControlPointTable
                        {
                            ControlPoints = Beatmap.Value.Beatmap.ControlPointInfo.AllControlPoints
                        }
                    }
                };
            }
        }

        public class ControlPointSettings : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = new Box
                {
                    Colour = colours.Gray3,
                    RelativeSizeAxes = Axes.Both,
                };
            }
        }
    }
}
