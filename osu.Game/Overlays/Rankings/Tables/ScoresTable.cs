// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users.Drawables;
using osuTK;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Rankings.Tables
{
    public class ScoresTable : RankingsTable<APIUserRankings>
    {
        public ScoresTable(int page = 1)
            : base(page)
        {
        }

        protected override TableColumn[] CreateHeaders() => new[]
        {
            new TableColumn("", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 50)), // place
            new TableColumn("", Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed)), // flag and username
            new TableColumn("Accuracy", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 80)),
            new TableColumn("Play Count", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 80)),
            new TableColumn("Total Score", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 100)),
            new TableColumn("Ranked Score", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 100)),
            new TableColumn("SS", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 70)),
            new TableColumn("S", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 70)),
            new TableColumn("A", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 70)),
        };

        protected override Drawable[] CreateContent(int index, APIUserRankings item)
        {
            var content = new List<Drawable>
            {
                new OsuSpriteText
                {
                    Text = $"#{index + 1}",
                    Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold)
                },
            };

            var username = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: TEXT_SIZE)) { AutoSizeAxes = Axes.Both };
            username.AddUserLink(item.User);

            content.Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(7, 0),
                Children = new Drawable[]
                {
                    new UpdateableFlag(item.User.Country)
                    {
                        Size = new Vector2(20, 13),
                        ShowPlaceholderOnNull = false,
                    },
                    username
                }
            });

            content.AddRange(new Drawable[]
            {
                new ColoredText
                {
                    Text = $@"{item.Accuracy:F2}%",
                    Font = OsuFont.GetFont(size: TEXT_SIZE),
                },
                new ColoredText
                {
                    Text = $@"{item.PlayCount:N0}",
                    Font = OsuFont.GetFont(size: TEXT_SIZE),
                },
                new ColoredMetricNumber(item.TotalScore)
                {
                    Font = OsuFont.GetFont(size: TEXT_SIZE),
                },
                new ColoredMetricNumber(item.RankedScore)
                {
                    Font = OsuFont.GetFont(size: TEXT_SIZE),
                },
                new ColoredText
                {
                    Text = $@"{item.GradesCount.SS + item.GradesCount.SSPlus:N0}",
                    Font = OsuFont.GetFont(size: TEXT_SIZE),
                },
                new ColoredText
                {
                    Text = $@"{item.GradesCount.S + item.GradesCount.SPlus:N0}",
                    Font = OsuFont.GetFont(size: TEXT_SIZE),
                },
                new ColoredText
                {
                    Text = $@"{item.GradesCount.A:N0}",
                    Font = OsuFont.GetFont(size: TEXT_SIZE),
                },
            });

            return content.ToArray();
        }

        protected override string HighlightedColumn() => @"Ranked Score";
    }
}
