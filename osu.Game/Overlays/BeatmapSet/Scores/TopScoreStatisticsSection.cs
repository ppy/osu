// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
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
        private readonly FontUsage largeFont = OsuFont.GetFont(size: 22, weight: FontWeight.Light);

        private readonly TextColumn totalScoreColumn;
        private readonly TextColumn accuracyColumn;
        private readonly TextColumn maxComboColumn;
        private readonly TextColumn ppColumn;

        private readonly FillFlowContainer<InfoColumn> statisticsColumns;
        private readonly ModsInfoColumn modsColumn;

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        public TopScoreStatisticsSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
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
                            totalScoreColumn = new TextColumn(BeatmapsetsStrings.ShowScoreboardHeadersScoreTotal, largeFont, top_columns_min_width),
                            accuracyColumn = new TextColumn(BeatmapsetsStrings.ShowScoreboardHeadersAccuracy, largeFont, top_columns_min_width),
                            maxComboColumn = new TextColumn(BeatmapsetsStrings.ShowScoreboardHeadersCombo, largeFont, top_columns_min_width)
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
                            ppColumn = new TextColumn(BeatmapsetsStrings.ShowScoreboardHeaderspp, smallFont, bottom_columns_min_width),
                            modsColumn = new ModsInfoColumn(),
                        }
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (score != null)
                totalScoreColumn.Current = scoreManager.GetBindableTotalScoreString(score);
        }

        private ScoreInfo score;

        /// <summary>
        /// Sets the score to be displayed.
        /// </summary>
        public ScoreInfo Score
        {
            set
            {
                if (score == null && value == null)
                    return;

                if (score?.Equals(value) == true)
                    return;

                score = value;

                accuracyColumn.Text = value.DisplayAccuracy;
                maxComboColumn.Text = value.MaxCombo.ToLocalisableString(@"0\x");

                ppColumn.Alpha = value.BeatmapInfo.Status.GrantsPerformancePoints() ? 1 : 0;
                ppColumn.Text = value.PP?.ToLocalisableString(@"N0");

                statisticsColumns.ChildrenEnumerable = value.GetStatisticsForDisplay().Select(createStatisticsColumn);
                modsColumn.Mods = value.Mods;

                if (scoreManager != null)
                    totalScoreColumn.Current = scoreManager.GetBindableTotalScoreString(value);
            }
        }

        private TextColumn createStatisticsColumn(HitResultDisplayStatistic stat) => new TextColumn(stat.DisplayName, smallFont, bottom_columns_min_width)
        {
            Text = stat.MaxCount == null ? stat.Count.ToLocalisableString(@"N0") : (LocalisableString)$"{stat.Count}/{stat.MaxCount}"
        };

        private class InfoColumn : CompositeDrawable
        {
            private readonly Box separator;
            private readonly OsuSpriteText text;

            public InfoColumn(LocalisableString title, Drawable content, float? minWidth = null)
            {
                AutoSizeAxes = Axes.Both;
                Margin = new MarginPadding { Vertical = 5 };

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
                        new Dimension(GridSizeMode.Absolute, 2),
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            text = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 10, weight: FontWeight.Bold),
                                Text = title.ToUpper(),
                                // 2px padding bottom + 1px vertical to compensate for the additional spacing because of 1.25 line-height in osu-web
                                Padding = new MarginPadding { Top = 1, Bottom = 3 }
                            }
                        },
                        new Drawable[]
                        {
                            separator = new Box
                            {
                                Anchor = Anchor.TopLeft,
                                RelativeSizeAxes = Axes.X,
                                Height = 2,
                            },
                        },
                        new[]
                        {
                            // osu-web has 4px margin here but also uses 0.9 line-height, reducing margin to 2px seems like a good alternative to that
                            content.With(c => c.Margin = new MarginPadding { Top = 2 })
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

            public TextColumn(LocalisableString title, FontUsage font, float? minWidth = null)
                : this(title, new OsuSpriteText { Font = font }, minWidth)
            {
            }

            private TextColumn(LocalisableString title, SpriteText text, float? minWidth = null)
                : base(title, text, minWidth)
            {
                this.text = text;
            }

            public LocalisableString Text
            {
                set => text.Text = value;
            }

            public Bindable<string> Current
            {
                get => text.Current;
                set => text.Current = value;
            }
        }

        private class ModsInfoColumn : InfoColumn
        {
            private readonly FillFlowContainer modsContainer;

            public ModsInfoColumn()
                : this(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(1),
                    Height = 18f
                })
            {
            }

            private ModsInfoColumn(FillFlowContainer modsContainer)
                : base(BeatmapsetsStrings.ShowScoreboardHeadersMods, modsContainer)
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
