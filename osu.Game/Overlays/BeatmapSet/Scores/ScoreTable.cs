// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTable : TableContainer
    {
        private const float horizontal_inset = 20;
        private const float row_height = 22;
        private const int text_size = 12;

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

        private bool showPerformancePoints;

        public void DisplayScores(IReadOnlyList<ScoreInfo> scores, bool showPerformanceColumn)
        {
            ClearScores();

            if (!scores.Any())
                return;

            showPerformancePoints = showPerformanceColumn;

            for (int i = 0; i < scores.Count; i++)
                backgroundFlow.Add(new ScoreTableRowBackground(i, scores[i], row_height));

            Columns = createHeaders(scores.FirstOrDefault());
            Content = scores.Select((s, i) => createContent(i, s)).ToArray().ToRectangular();
        }

        public void ClearScores()
        {
            Content = null;
            backgroundFlow.Clear();
        }

        private TableColumn[] createHeaders(ScoreInfo score)
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

            foreach (var statistic in score.SortedStatistics.Take(score.SortedStatistics.Count() - 1))
                columns.Add(new TableColumn(statistic.Key.GetDescription(), Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed, minSize: 35, maxSize: 60)));

            columns.Add(new TableColumn(score.SortedStatistics.LastOrDefault().Key.GetDescription(), Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed, minSize: 45, maxSize: 95)));

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
                    Text = $@"{score.TotalScore:N0}",
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

            foreach (var kvp in score.SortedStatistics)
            {
                content.Add(new OsuSpriteText
                {
                    Text = $"{kvp.Value}",
                    Font = OsuFont.GetFont(size: text_size),
                    Colour = kvp.Value == 0 ? Color4.Gray : Color4.White
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
