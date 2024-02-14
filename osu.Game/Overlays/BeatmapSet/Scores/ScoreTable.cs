// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.EnumExtensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public partial class ScoreTable : TableContainer
    {
        private const float horizontal_inset = 20;
        private const float row_height = 22;
        private const int text_size = 12;

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        private readonly FillFlowContainer backgroundFlow;

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

        /// <summary>
        /// The statistics that appear in the table, in order of appearance.
        /// </summary>
        private readonly List<(HitResult result, LocalisableString displayName)> statisticResultTypes = new List<(HitResult, LocalisableString)>();

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
                new TableColumn(BeatmapsetsStrings.ShowScoreboardHeadersRank, Anchor.CentreRight, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 70)), // grade
                new TableColumn(BeatmapsetsStrings.ShowScoreboardHeadersScore, Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn(BeatmapsetsStrings.ShowScoreboardHeadersAccuracy, Anchor.CentreLeft, new Dimension(GridSizeMode.Absolute, minSize: 60, maxSize: 70)),
                new TableColumn("", Anchor.CentreLeft, new Dimension(GridSizeMode.Absolute, 25)), // flag
                new TableColumn(BeatmapsetsStrings.ShowScoreboardHeadersPlayer, Anchor.CentreLeft, new Dimension(minSize: 125)),
                new TableColumn(BeatmapsetsStrings.ShowScoreboardHeadersCombo, Anchor.CentreLeft, new Dimension(minSize: 70, maxSize: 120))
            };

            // All statistics across all scores, unordered.
            var allScoreStatistics = scores.SelectMany(s => s.GetStatisticsForDisplay().Select(stat => stat.Result)).ToHashSet();

            var ruleset = scores.First().Ruleset.CreateInstance();

            foreach (var result in EnumExtensions.GetValuesInOrder<HitResult>())
            {
                if (!allScoreStatistics.Contains(result))
                    continue;

                // for the time being ignore bonus result types.
                // this is not being sent from the API and will be empty in all cases.
                if (result.IsBonus())
                    continue;

                var displayName = ruleset.GetDisplayNameForHitResult(result);

                columns.Add(new TableColumn(displayName, Anchor.CentreLeft, new Dimension(minSize: 35, maxSize: 60)));
                statisticResultTypes.Add((result, displayName));
            }

            if (showPerformancePoints)
                columns.Add(new TableColumn(BeatmapsetsStrings.ShowScoreboardHeaderspp, Anchor.CentreLeft, new Dimension(GridSizeMode.Absolute, 30)));

            columns.Add(new TableColumn(BeatmapsetsStrings.ShowScoreboardHeadersTime, Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)));
            columns.Add(new TableColumn(BeatmapsetsStrings.ShowScoreboardHeadersMods, Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)));

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
                    Text = (index + 1).ToLocalisableString(@"\##"),
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
                new StatisticText(score.Accuracy, 1, showTooltip: false)
                {
                    Margin = new MarginPadding { Right = horizontal_inset },
                    Text = score.DisplayAccuracy,
                },
                new UpdateableFlag(score.User.CountryCode)
                {
                    Size = new Vector2(19, 14),
                    ShowPlaceholderOnUnknown = false,
                },
                username,
#pragma warning disable 618
                new StatisticText(score.MaxCombo, score.BeatmapInfo!.MaxCombo, @"0\x"),
#pragma warning restore 618
            };

            var availableStatistics = score.GetStatisticsForDisplay().ToDictionary(tuple => tuple.Result);

            foreach (var result in statisticResultTypes)
            {
                if (!availableStatistics.TryGetValue(result.result, out var stat))
                    stat = new HitResultDisplayStatistic(result.result, 0, null, result.displayName);

                content.Add(new StatisticText(stat.Count, stat.MaxCount, @"N0") { Colour = stat.Count == 0 ? Color4.Gray : Color4.White });
            }

            if (showPerformancePoints)
            {
                if (!score.Ranked)
                {
                    content.Add(new SpriteTextWithTooltip
                    {
                        Text = "-",
                        Font = OsuFont.GetFont(size: text_size),
                        TooltipText = ScoresStrings.StatusNoPp
                    });
                }
                else if (score.PP == null)
                {
                    content.Add(new SpriteIconWithTooltip
                    {
                        Icon = FontAwesome.Solid.Sync,
                        Size = new Vector2(text_size),
                        TooltipText = ScoresStrings.StatusProcessing,
                    });
                }
                else
                    content.Add(new StatisticText(score.PP, format: @"N0"));
            }

            content.Add(new ScoreboardTime(score.Date, text_size)
            {
                Margin = new MarginPadding { Right = 10 }
            });

            content.Add(new FillFlowContainer
            {
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(1),
                ChildrenEnumerable = score.Mods.AsOrdered().Select(m => new ModIcon(m)
                {
                    AutoSizeAxes = Axes.Both,
                    Scale = new Vector2(0.3f)
                })
            });

            return content.ToArray();
        }

        protected override Drawable CreateHeader(int index, TableColumn column) => new HeaderText(column?.Header ?? default);

        private partial class HeaderText : OsuSpriteText
        {
            public HeaderText(LocalisableString text)
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

        private partial class StatisticText : OsuSpriteText, IHasTooltip
        {
            private readonly double? count;
            private readonly double? maxCount;
            private readonly bool showTooltip;

            public LocalisableString TooltipText => maxCount == null || !showTooltip ? string.Empty : $"{count}/{maxCount}";

            public StatisticText(double? count, double? maxCount = null, string format = null, bool showTooltip = true)
            {
                this.count = count;
                this.maxCount = maxCount;
                this.showTooltip = showTooltip;

                Text = count?.ToLocalisableString(format) ?? default;
                Font = OsuFont.GetFont(size: text_size);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                if (count == maxCount)
                    Colour = colours.GreenLight;
            }
        }
    }
}
