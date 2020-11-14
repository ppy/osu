// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using JetBrains.Annotations;
using static osu.Game.Users.User;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class ProfileLineChart : CompositeDrawable
    {
        private UserHistoryCount[] values;

        [CanBeNull]
        public UserHistoryCount[] Values
        {
            get => values;
            set
            {
                values = value;
                graph.Values = values;
            }
        }

        private readonly UserHistoryGraph graph;

        public ProfileLineChart()
        {
            RelativeSizeAxes = Axes.X;
            Height = 250;
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension()
                },
                RowDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        Empty(),
                        graph = new UserHistoryGraph
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    },
                    new Drawable[]
                    {
                        Empty(),
                        Empty()
                    }
                }
            };
        }
    }
}
