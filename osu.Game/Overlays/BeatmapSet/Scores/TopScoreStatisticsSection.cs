// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class TopScoreStatisticsSection : CompositeDrawable
    {
        private const float margin = 10;
        private const float top_columns_min_width = 64;
        private const float bottom_columns_min_width = 45;

        private readonly FontUsage smallFont = OsuFont.GetFont(size: 16);
        private readonly FontUsage largeFont = OsuFont.GetFont(size: 22);

        private readonly TextColumn totalScoreColumn;
        private readonly TextColumn accuracyColumn;
        private readonly TextColumn maxComboColumn;
        private readonly TextColumn ppColumn;

        private readonly FillFlowContainer<InfoColumn> statisticsColumns;
        private readonly ModsInfoColumn modsColumn;

        public TopScoreStatisticsSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10, 8),
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(margin, 0),
                        Children = new Drawable[]
                        {
                            totalScoreColumn = new TextColumn("total score", largeFont, top_columns_min_width),
                            accuracyColumn = new TextColumn("accuracy", largeFont, top_columns_min_width),
                            maxComboColumn = new TextColumn("max combo", largeFont, top_columns_min_width)
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(margin, 0),
                        Children = new Drawable[]
                        {
                            statisticsColumns = new FillFlowContainer<InfoColumn>
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(margin, 0),
                            },
                            ppColumn = new TextColumn("pp", smallFont, bottom_columns_min_width),
                            modsColumn = new ModsInfoColumn(),
                        }
                    },
                }
            };
        }

        /// <summary>
        /// Sets the score to be displayed.
        /// </summary>
        public ScoreInfo Score
        {
            set
            {
                totalScoreColumn.Text = $@"{value.TotalScore:N0}";
                accuracyColumn.Text = value.DisplayAccuracy;
                maxComboColumn.Text = $@"{value.MaxCombo:N0}x";
                ppColumn.Text = $@"{value.PP:N0}";

                statisticsColumns.ChildrenEnumerable = value.SortedStatistics.Select(kvp => createStatisticsColumn(kvp.Key, kvp.Value));
                modsColumn.Mods = value.Mods;
            }
        }

        private TextColumn createStatisticsColumn(HitResult hitResult, int count) => new TextColumn(hitResult.GetDescription(), smallFont, bottom_columns_min_width)
        {
            Text = count.ToString()
        };

        private class InfoColumn : CompositeDrawable
        {
            private readonly Box separator;
            private readonly OsuSpriteText text;

            public InfoColumn(string title, Drawable content, float? minWidth = null)
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new GridContainer
                {
                    AutoSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize, minSize: minWidth ?? 0)
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Absolute, 4),
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            text = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 10, weight: FontWeight.Bold),
                                Text = title.ToUpper()
                            }
                        },
                        new Drawable[]
                        {
                            separator = new Box
                            {
                                Anchor = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                                Height = 2
                            }
                        },
                        new[]
                        {
                            content
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                text.Colour = colourProvider.Foreground1;
                separator.Colour = colourProvider.Background3;
            }
        }

        private class TextColumn : InfoColumn
        {
            private readonly SpriteText text;

            public TextColumn(string title, FontUsage font, float? minWidth = null)
                : this(title, new OsuSpriteText { Font = font }, minWidth)
            {
            }

            private TextColumn(string title, SpriteText text, float? minWidth = null)
                : base(title, text, minWidth)
            {
                this.text = text;
            }

            public LocalisedString Text
            {
                set => text.Text = value;
            }
        }

        private class ModsInfoColumn : InfoColumn
        {
            private readonly FillFlowContainer modsContainer;

            public ModsInfoColumn()
                : this(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(1),
                })
            {
            }

            private ModsInfoColumn(FillFlowContainer modsContainer)
                : base("mods", modsContainer)
            {
                this.modsContainer = modsContainer;
            }

            public IEnumerable<Mod> Mods
            {
                set
                {
                    modsContainer.Clear();
                    modsContainer.Children = value.Select(mod => new ModIcon(mod)
                    {
                        AutoSizeAxes = Axes.Both,
                        Scale = new Vector2(0.25f),
                    }).ToList();
                }
            }
        }
    }
}
