// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using osu.Game.Users;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using System.Collections.Generic;

namespace osu.Game.Overlays.Rankings.Tables
{
    public class CountriesTable : RankingsTable<CountryStatistics>
    {
        public CountriesTable(int page, IReadOnlyList<CountryStatistics> rankings)
            : base(page, rankings)
        {
        }

        protected override TableColumn[] CreateAdditionalHeaders() => new[]
        {
            new TableColumn("Active Users", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Play Count", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Ranked Score", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Avg. Score", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Performance", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Avg. Perf.", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
        };

        protected override Country GetCountry(CountryStatistics item) => item.Country;

        protected override Drawable CreateFlagContent(CountryStatistics item) => new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: TEXT_SIZE),
            Text = $@"{item.Country.FullName}",
        };

        protected override Drawable[] CreateAdditionalContent(CountryStatistics item) => new Drawable[]
        {
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
