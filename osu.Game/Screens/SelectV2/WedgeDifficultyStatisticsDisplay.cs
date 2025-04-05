// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class WedgeDifficultyStatisticsDisplay : CompositeDrawable, IHasAccentColour
    {
        private readonly bool autosize;
        private readonly FillFlowContainer<WedgeStatisticDifficulty> statisticsFlow;
        private readonly GridContainer tinyStatisticsGrid;

        private IReadOnlyList<WedgeStatisticDifficulty.Data>? statistics;

        public IReadOnlyList<WedgeStatisticDifficulty.Data>? Statistics
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

        public WedgeDifficultyStatisticsDisplay(bool autosize = false)
        {
            this.autosize = autosize;

            if (autosize)
                AutoSizeAxes = Axes.Both;
            else
                AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                statisticsFlow = new FillFlowContainer<WedgeStatisticDifficulty>
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(8f, 0f),
                    Direction = FillDirection.Horizontal,
                    AlwaysPresent = true,
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

        [Resolved]
        private LocalisationManager localisations { get; set; } = null!;

        private IBindable<LocalisationParameters>? localisationParameters;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            localisationParameters = localisations.CurrentParameters.GetBoundCopy();
            localisationParameters.BindValueChanged(_ => updateStatisticsSizing());
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

        private bool displayedTinyStatistics;

        private void updateLayout()
        {
            if (statistics?.Count < 1)
                return;

            float flowWidth = statisticsFlow[0].Width * statisticsFlow.Count + statisticsFlow.Spacing.X * (statisticsFlow.Count - 1);
            bool tiny = !autosize && DrawWidth < flowWidth;

            if (displayedTinyStatistics != tiny)
            {
                if (tiny)
                {
                    statisticsFlow.Hide();
                    tinyStatisticsGrid.FadeIn(200, Easing.InQuint);
                }
                else
                {
                    tinyStatisticsGrid.Hide();
                    statisticsFlow.FadeIn(200, Easing.InQuint);
                }

                displayedTinyStatistics = tiny;
            }
        }

        private void updateStatisticsSizing() => SchedulerAfterChildren.AddOnce(() =>
        {
            if (statisticsFlow.Count == 0)
                return;

            float statisticWidth = Math.Max(65, statisticsFlow.Max(s => s.LabelWidth));

            foreach (var statistic in statisticsFlow)
                statistic.Width = statisticWidth;

            drawSizeLayout.Invalidate();
        });

        private void updateStatistics()
        {
            if (statistics == null)
                return;

            var newStatistics = statistics.Select(data => new WedgeStatisticDifficulty
            {
                Value = data,
            }).ToArray();

            var currentStatistics = statisticsFlow.Children;

            if (currentStatistics.Select(s => s.Value.Label).SequenceEqual(newStatistics.Select(s => s.Value.Label)))
            {
                for (int i = 0; i < newStatistics.Length; i++)
                    currentStatistics[i].Value = newStatistics[i].Value;
            }
            else
            {
                statisticsFlow.Children = newStatistics;
                updateStatisticsSizing();
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
                    Text = s.Label,
                    Font = OsuFont.Tiny.With(weight: FontWeight.SemiBold),
                    Colour = colourProvider.Content2,
                },
                Empty(),
                new OsuSpriteText
                {
                    Font = OsuFont.Tiny.With(weight: FontWeight.SemiBold),
                    Text = s.Value.ToLocalisableString("0.##"),
                    Colour = colourProvider.Content1,
                },
            }).ToArray();
        }
    }
}
