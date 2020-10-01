﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Users.Drawables;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTable : TableContainer
    {
        private const float horizontal_inset = 20;
        private const float row_height = 22;
        private const int text_size = 12;

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        private readonly FillFlowContainer backgroundFlow;

        private Color4 highAccuracyColour;

        public ScoreTable()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Horizontal = horizontal_inset };
            RowSize = new Dimension(GridSizeMode.Absolute, row_height);

            AddInternal(backgroundFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Depth = 1f,
                Padding = new MarginPadding { Horizontal = -horizontal_inset },
                Margin = new MarginPadding { Top = row_height }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            highAccuracyColour = colours.GreenLight;
        }

        /// <summary>
        /// The statistics that appear in the table, in order of appearance.
        /// </summary>
        private readonly List<HitResult> statisticResultTypes = new List<HitResult>();

        private bool showPerformancePoints;

        public void DisplayScores(IReadOnlyList<ScoreInfo> scores, bool showPerformanceColumn)
        {
            ClearScores();

            if (!scores.Any())
                return;

            showPerformancePoints = showPerformanceColumn;
            statisticResultTypes.Clear();

            for (int i = 0; i < scores.Count; i++)
                backgroundFlow.Add(new ScoreTableRowBackground(i, scores[i], row_height));

            Columns = createHeaders(scores);
            Content = scores.Select((s, i) => createContent(i, s)).ToArray().ToRectangular();
        }

        public void ClearScores()
        {
            Content = null;
            backgroundFlow.Clear();
        }

        private TableColumn[] createHeaders(IReadOnlyList<ScoreInfo> scores)
        {
            var columns = new List<TableColumn>
            {
                new TableColumn("rank", Anchor.CentreRight, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 70)), // grade
                new TableColumn("score", Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("accuracy", Anchor.CentreLeft, new Dimension(GridSizeMode.Absolute, minSize: 60, maxSize: 70)),
                new TableColumn("", Anchor.CentreLeft, new Dimension(GridSizeMode.Absolute, 25)), // flag
                new TableColumn("player", Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed, minSize: 125)),
                new TableColumn("max combo", Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed, minSize: 70, maxSize: 120))
            };

            // All statistics across all scores, unordered.
            var allScoreStatistics = scores.SelectMany(s => s.GetStatisticsForDisplay().Select(stat => stat.result)).ToHashSet();

            foreach (var result in OrderAttributeUtils.GetValuesInOrder<HitResult>())
            {
                if (!allScoreStatistics.Contains(result))
                    continue;

                columns.Add(new TableColumn(result.GetDescription(), Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed, minSize: 35, maxSize: 60)));
                statisticResultTypes.Add(result);
            }

            if (showPerformancePoints)
                columns.Add(new TableColumn("pp", Anchor.CentreLeft, new Dimension(GridSizeMode.Absolute, 30)));

            columns.Add(new TableColumn("mods", Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)));

            return columns.ToArray();
        }

        private Drawable[] createContent(int index, ScoreInfo score)
        {
            var username = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: text_size)) { AutoSizeAxes = Axes.Both };
            username.AddUserLink(score.User);

            var content = new List<Drawable>
            {
                new OsuSpriteText
                {
                    Text = $"#{index + 1}",
                    Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                },
                new UpdateableRank(score.Rank)
                {
                    Size = new Vector2(28, 14)
                },
                new OsuSpriteText
                {
                    Margin = new MarginPadding { Right = horizontal_inset },
                    Current = scoreManager.GetBindableTotalScoreString(score),
                    Font = OsuFont.GetFont(size: text_size, weight: index == 0 ? FontWeight.Bold : FontWeight.Medium)
                },
                new OsuSpriteText
                {
                    Margin = new MarginPadding { Right = horizontal_inset },
                    Text = score.DisplayAccuracy,
                    Font = OsuFont.GetFont(size: text_size),
                    Colour = score.Accuracy == 1 ? highAccuracyColour : Color4.White
                },
                new UpdateableFlag(score.User.Country)
                {
                    Size = new Vector2(19, 13),
                    ShowPlaceholderOnNull = false,
                },
                username,
                new OsuSpriteText
                {
                    Text = $@"{score.MaxCombo:N0}x",
                    Font = OsuFont.GetFont(size: text_size),
                    Colour = score.MaxCombo == score.Beatmap?.MaxCombo ? highAccuracyColour : Color4.White
                }
            };

            var availableStatistics = score.GetStatisticsForDisplay().ToDictionary(tuple => tuple.result);

            foreach (var result in statisticResultTypes)
            {
                if (!availableStatistics.TryGetValue(result, out var stat))
                    stat = (result, 0, null);

                content.Add(new OsuSpriteText
                {
                    Text = stat.maxCount == null ? $"{stat.count}" : $"{stat.count}/{stat.maxCount}",
                    Font = OsuFont.GetFont(size: text_size),
                    Colour = stat.count == 0 ? Color4.Gray : Color4.White
                });
            }

            if (showPerformancePoints)
            {
                content.Add(new OsuSpriteText
                {
                    Text = $@"{score.PP:N0}",
                    Font = OsuFont.GetFont(size: text_size)
                });
            }

            content.Add(new FillFlowContainer
            {
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(1),
                ChildrenEnumerable = score.Mods.Select(m => new ModIcon(m)
                {
                    AutoSizeAxes = Axes.Both,
                    Scale = new Vector2(0.3f)
                })
            });

            return content.ToArray();
        }

        protected override Drawable CreateHeader(int index, TableColumn column) => new HeaderText(column?.Header ?? string.Empty);

        private class HeaderText : OsuSpriteText
        {
            public HeaderText(string text)
            {
                Text = text.ToUpper();
                Font = OsuFont.GetFont(size: 10, weight: FontWeight.Bold);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Foreground1;
            }
        }
    }
}
