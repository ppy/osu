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
    public partial class BeatmapTitleWedge
    {
        public partial class DifficultyStatisticsDisplay : CompositeDrawable
        {
            private readonly bool autoSize;
            private readonly FillFlowContainer<StatisticDifficulty> statisticsFlow;
            private readonly GridContainer tinyStatisticsGrid;

            private IReadOnlyList<StatisticDifficulty.Data> statistics = Array.Empty<StatisticDifficulty.Data>();

            public IReadOnlyList<StatisticDifficulty.Data> Statistics
            {
                get => statistics;
                set
                {
                    statistics = value;

                    if (IsLoaded)
                    {
                        updateStatistics();
                        updateTinyStatistics();
                    }
                }
            }

            private Color4 accentColour;

            public Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    if (accentColour == value)
                        return;

                    accentColour = value;

                    foreach (var statistic in statisticsFlow)
                        statistic.AccentColour = value;
                }
            }

            private readonly LayoutValue drawSizeLayout = new LayoutValue(Invalidation.DrawSize);

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public DifficultyStatisticsDisplay(bool autoSize = false)
            {
                this.autoSize = autoSize;

                if (autoSize)
                    AutoSizeAxes = Axes.Both;
                else
                    AutoSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    statisticsFlow = new FillFlowContainer<StatisticDifficulty>
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
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
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

                updateStatistics();
                updateTinyStatistics();
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
                if (statisticsFlow.Count == 0)
                    return;

                float flowWidth = statisticsFlow[0].Width * statisticsFlow.Count + statisticsFlow.Spacing.X * (statisticsFlow.Count - 1);
                bool tiny = !autoSize && DrawWidth < flowWidth - 20;

                if (displayedTinyStatistics != tiny)
                {
                    if (tiny)
                    {
                        statisticsFlow.Hide();
                        // Slow fade hides fill flow layout weirdness.
                        tinyStatisticsGrid.FadeIn(200, Easing.InQuint);
                    }
                    else
                    {
                        tinyStatisticsGrid.Hide();
                        // Slow fade hides fill flow layout weirdness.
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
                {
                    statistic.Width = statisticWidth;
                    // Slow fade hides fill flow layout weirdness.
                    statistic.FadeIn(200, Easing.InQuint);
                }

                drawSizeLayout.Invalidate();
            });

            private void updateStatistics() => Scheduler.AddOnce(() =>
            {
                if (statisticsFlow.Select(s => s.Value.Label)
                                  .SequenceEqual(statistics.Select(s => s.Label)))
                {
                    for (int i = 0; i < statistics.Count; i++)
                        statisticsFlow[i].Value = statistics[i];
                }
                else
                {
                    statisticsFlow.ChildrenEnumerable = statistics.Select(d => new StatisticDifficulty
                    {
                        Alpha = 0,
                        AccentColour = accentColour,
                        Value = d
                    });
                    updateStatisticsSizing();
                }
            });

            private void updateTinyStatistics()
            {
                tinyStatisticsGrid.RowDimensions = statistics.Select(_ => new Dimension(GridSizeMode.AutoSize)).ToArray();
                tinyStatisticsGrid.Content = statistics.Select(s => new[]
                {
                    new OsuSpriteText
                    {
                        Text = s.Label,
                        Font = OsuFont.Style.Caption2.With(weight: FontWeight.SemiBold),
                        Colour = colourProvider.Content2,
                    },
                    Empty(),
                    new OsuSpriteText
                    {
                        Font = OsuFont.Style.Caption2.With(weight: FontWeight.SemiBold),
                        Text = s.Content ?? s.Value.ToLocalisableString("0.##"),
                        Colour = colourProvider.Content1,
                    },
                }).ToArray();
            }
        }
    }
}
