// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Edit.Verify
{
    public class VerifyScreen : EditorScreenWithTimeline
    {
        public VerifyScreen()
            : base(EditorScreenMode.Verify)
        {
        }

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
                    new ControlPointList()
                },
            }
        };

        public class ControlPointList : CompositeDrawable
        {
            private IssueTable table;

            [Resolved]
            private EditorClock clock { get; set; }

            [Resolved]
            protected EditorBeatmap Beatmap { get; private set; }

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
                        Child = table = new IssueTable(),
                    }
                };
            }
        }
    }
}
