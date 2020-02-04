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
        private const float row_height = 25;
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

        public IReadOnlyList<ScoreInfo> Scores
        {
            set
            {
                Content = null;
                backgroundFlow.Clear();

                if (value?.Any() != true)
                    return;

                for (int i = 0; i < value.Count; i++)
                    backgroundFlow.Add(new ScoreTableRowBackground(i, value[i]));

                Columns = createHeaders(value[0]);
                Content = value.Select((s, i) => createContent(i, s)).ToArray().ToRectangular();
            }
        }

        private TableColumn[] createHeaders(ScoreInfo score)
        {
            var columns = new List<TableColumn>
            {
                new TableColumn("rank", Anchor.CentreRight, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 70)), // grade
                new TableColumn("score", Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("accuracy", Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("player", Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed, minSize: 150)),
                new TableColumn("max combo", Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed, minSize: 70, maxSize: 90))
            };

            foreach (var statistic in score.Statistics)
                columns.Add(new TableColumn(statistic.Key.GetDescription(), Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed, minSize: 50, maxSize: 70)));

            columns.AddRange(new[]
            {
                new TableColumn("pp", Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed, minSize: 40, maxSize: 70)),
                new TableColumn("mods", Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)),
            });

            return columns.ToArray();
        }

        private Drawable[] createContent(int index, ScoreInfo score)
        {
            var content = new List<Drawable>
            {
                new OsuSpriteText
                {
                    Text = $"#{index + 1}",
                    Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                },
                new UpdateableRank(score.Rank)
                {
                    Size = new Vector2(30, 20)
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
            };

            var username = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: text_size)) { AutoSizeAxes = Axes.Both };
            username.AddUserLink(score.User);

            content.AddRange(new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Right = horizontal_inset },
                    Spacing = new Vector2(5, 0),
                    Children = new Drawable[]
                    {
                        new UpdateableFlag(score.User.Country)
                        {
                            Size = new Vector2(20, 13),
                            ShowPlaceholderOnNull = false,
                        },
                        username
                    }
                },
                new OsuSpriteText
                {
                    Text = $@"{score.MaxCombo:N0}x",
                    Font = OsuFont.GetFont(size: text_size)
                }
            });

            foreach (var kvp in score.Statistics)
            {
                content.Add(new OsuSpriteText
                {
                    Text = $"{kvp.Value}",
                    Font = OsuFont.GetFont(size: text_size),
                    Colour = kvp.Value == 0 ? Color4.Gray : Color4.White
                });
            }

            content.AddRange(new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = $@"{score.PP:N0}",
                    Font = OsuFont.GetFont(size: text_size)
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(1),
                    ChildrenEnumerable = score.Mods.Select(m => new ModIcon(m)
                    {
                        AutoSizeAxes = Axes.Both,
                        Scale = new Vector2(0.3f)
                    })
                },
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
