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
            new TableColumn("", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 50)), // place
            new TableColumn("", Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed)), // flag and country name
            new TableColumn("Active Users", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 100)),
            new TableColumn("Play Count", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 100)),
            new TableColumn("Ranked Score", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 100)),
            new TableColumn("Avg. Score", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 100)),
            new TableColumn("Performance", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 80)),
            new TableColumn("Avg. Perf.", Anchor.Centre, new Dimension(GridSizeMode.Absolute, 50)),
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
                    new OsuSpriteText
                    {
                        Text = $@"{item.Country.FullName}",
                        Font = OsuFont.GetFont(size: TEXT_SIZE),
                    }
                }
            },
            new ColoredText
            {
                Text = $@"{item.ActiveUsers:N0}",
                Font = OsuFont.GetFont(size: TEXT_SIZE),
            },
            new ColoredMetricNumber(item.PlayCount)
            {
                Font = OsuFont.GetFont(size: TEXT_SIZE),
            },
            new ColoredMetricNumber(item.RankedScore)
            {
                Font = OsuFont.GetFont(size: TEXT_SIZE),
            },
            new ColoredMetricNumber(item.RankedScore / Math.Max(item.ActiveUsers, 1))
            {
                Font = OsuFont.GetFont(size: TEXT_SIZE),
            },
            new MetricNumber(item.Performance)
            {
                Font = OsuFont.GetFont(size: TEXT_SIZE),
            },
            new ColoredText
            {
                Text = $@"{item.Performance / Math.Max(item.ActiveUsers, 1):N0}",
                Font = OsuFont.GetFont(size: TEXT_SIZE),
            }
        };
    }
}
