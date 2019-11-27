// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users.Drawables;
using osuTK;
using osu.Game.Online.API.Requests.Responses;
using System;

namespace osu.Game.Overlays.Rankings.Tables
{
    public class CountriesTable : RankingsTable<APICountryRankings>
    {
        public CountriesTable(int page = 1)
            : base(page)
        {
        }

        protected override TableColumn[] CreateHeaders() => new[]
        {
            new TableColumn(string.Empty, Anchor.Centre, new Dimension(GridSizeMode.Absolute, 50)), // place
            new TableColumn(string.Empty, Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed)), // flag and country name
            new TableColumn("Active Users", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Play Count", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Ranked Score", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Avg. Score", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Performance", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Avg. Perf.", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
        };

        protected override Drawable[] CreateContent(int index, APICountryRankings item) => new Drawable[]
        {
            new OsuSpriteText
            {
                Text = $"#{index + 1}",
                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold)
            },
            new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(7, 0),
                Children = new Drawable[]
                {
                    new UpdateableFlag(item.Country)
                    {
                        Size = new Vector2(20, 13),
                        ShowPlaceholderOnNull = false,
                    },
                    new RowText
                    {
                        Text = $@"{item.Country.FullName}",
                    }
                }
            },
            new ColoredRowText
            {
                Text = $@"{item.ActiveUsers:N0}",
            },
            new ColoredRowText
            {
                Text = $@"{item.PlayCount:N0}",
            },
            new ColoredRowText
            {
                Text = $@"{item.RankedScore:N0}",
            },
            new ColoredRowText
            {
                Text = $@"{item.RankedScore / Math.Max(item.ActiveUsers, 1):N0}",
            },
            new RowText
            {
                Text = $@"{item.Performance:N0}",
            },
            new ColoredRowText
            {
                Text = $@"{item.Performance / Math.Max(item.ActiveUsers, 1):N0}",
            }
        };
    }
}
