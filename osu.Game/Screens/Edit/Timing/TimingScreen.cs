// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing
{
    public class TimingScreen : EditorScreenWithTimeline
    {
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
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new ControlPointRow(),
                                new ControlPointRow(),
                                new ControlPointRow(),
                                new ControlPointRow(),
                                new ControlPointRow(),
                                new ControlPointRow(),
                            }
                        },
                    }
                };
            }

            private class ControlPointRow : CompositeDrawable
            {
                public ControlPointRow()
                {
                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChildren = new Drawable[]
                    {
                        new OsuSpriteText { Text = "sample row" },
                    };
                }
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
