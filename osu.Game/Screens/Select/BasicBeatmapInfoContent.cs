// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Select
{
    public partial class BasicBeatmapInfoContent : Container
    {
        public BasicBeatmapInfoContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 10), // padding
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    new DifficultyNameContent
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                    Empty(),
                                    new LengthAndBPMStatisticPill
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                    }
                                }
                            }
                        },
                        // TODO: add density and fail graph content
                    },
                },
            };
        }
    }
}
