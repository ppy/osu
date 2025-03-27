// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class WedgeHitStatisticsDisplay : CompositeDrawable, IHasAccentColour
    {
        private const float statistic_width = 75;

        private readonly FillFlowContainer<WedgeStatisticDifficulty> statisticsFlow;
        private readonly GridContainer tinyStatisticsGrid;

        private IReadOnlyList<BeatmapStatistic>? statistics;

        public IReadOnlyList<BeatmapStatistic>? Statistics
        {
            get => statistics;
            set
            {
                statistics = value;
                updateStatistics();
                updateTinyStatistics();
            }
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;

                foreach (var statistic in statisticsFlow)
                    statistic.AccentColour = value;
            }
        }

        private readonly LayoutValue drawSizeLayout = new LayoutValue(Invalidation.DrawSize);

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public WedgeHitStatisticsDisplay()
        {
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                statisticsFlow = new FillFlowContainer<WedgeStatisticDifficulty>
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(8f, 0f),
                    Direction = FillDirection.Horizontal,
                },
                tinyStatisticsGrid = new GridContainer
                {
                    Alpha = 0f,
                    AutoSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Absolute, 8),
                        new Dimension(GridSizeMode.AutoSize),
                    }
                },
            };

            AddLayout(drawSizeLayout);
        }

        protected override void Update()
        {
            base.Update();

            if (!drawSizeLayout.IsValid)
            {
                updateLayout();
                drawSizeLayout.Validate();
            }
        }

        private void updateLayout()
        {
            if (statistics == null)
                return;

            float statisticsFlowWidth = statistic_width * statistics.Count + 8 * (statistics.Count - 1);
            bool displayTinyStatistics = DrawWidth < statisticsFlowWidth;
            statisticsFlow.Alpha = displayTinyStatistics ? 0 : 1;
            tinyStatisticsGrid.Alpha = displayTinyStatistics ? 1 : 0;
        }

        private void updateStatistics()
        {
            if (statistics == null)
                return;

            var newStatistics = statistics.Select(s => new WedgeStatisticDifficulty(s.Name)
            {
                Value = (s.Count, s.Maximum),
                Width = statistic_width,
            }).ToArray();

            var currentStatistics = statisticsFlow.Children;

            if (currentStatistics.Select(s => s.Label).SequenceEqual(newStatistics.Select(s => s.Label)))
            {
                for (int i = 0; i < newStatistics.Length; i++)
                    currentStatistics[i].Value = newStatistics[i].Value;
            }
            else
            {
                statisticsFlow.Children = newStatistics;
                drawSizeLayout.Invalidate();
            }
        }

        private void updateTinyStatistics()
        {
            if (statistics == null)
                return;

            tinyStatisticsGrid.RowDimensions = statistics.Select(s => new Dimension(GridSizeMode.AutoSize)).ToArray();
            tinyStatisticsGrid.Content = statistics.Select(s => new[]
            {
                new OsuSpriteText
                {
                    Text = s.Name,
                    Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.SemiBold),
                    Colour = colourProvider.Content2,
                },
                Empty(),
                new OsuSpriteText
                {
                    Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.SemiBold),
                    Text = s.Content,
                    Colour = colourProvider.Content1,
                },
            }).ToArray();

            drawSizeLayout.Invalidate();
        }
    }
}
