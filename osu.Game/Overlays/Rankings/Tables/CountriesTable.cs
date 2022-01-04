// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using osu.Game.Users;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Rankings.Tables
{
    public class CountriesTable : RankingsTable<CountryStatistics>
    {
        public CountriesTable(int page, IReadOnlyList<CountryStatistics> rankings)
            : base(page, rankings)
        {
        }

        protected override RankingsTableColumn[] CreateAdditionalHeaders() => new[]
        {
            new RankingsTableColumn(RankingsStrings.StatActiveUsers, Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new RankingsTableColumn(RankingsStrings.StatPlayCount, Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new RankingsTableColumn(RankingsStrings.StatRankedScore, Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new RankingsTableColumn(RankingsStrings.StatAverageScore, Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new RankingsTableColumn(RankingsStrings.StatPerformance, Anchor.Centre, new Dimension(GridSizeMode.AutoSize), true),
            new RankingsTableColumn(RankingsStrings.StatAveragePerformance, Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
        };

        protected override Country GetCountry(CountryStatistics item) => item.Country;

        protected override Drawable CreateFlagContent(CountryStatistics item) => new CountryName(item.Country);

        protected override Drawable[] CreateAdditionalContent(CountryStatistics item) => new Drawable[]
        {
            new ColouredRowText
            {
                Text = item.ActiveUsers.ToLocalisableString(@"N0")
            },
            new ColouredRowText
            {
                Text = item.PlayCount.ToLocalisableString(@"N0")
            },
            new ColouredRowText
            {
                Text = item.RankedScore.ToLocalisableString(@"N0")
            },
            new ColouredRowText
            {
                Text = (item.RankedScore / Math.Max(item.ActiveUsers, 1)).ToLocalisableString(@"N0")
            },
            new RowText
            {
                Text = item.Performance.ToLocalisableString(@"N0")
            },
            new ColouredRowText
            {
                Text = (item.Performance / Math.Max(item.ActiveUsers, 1)).ToLocalisableString(@"N0")
            }
        };

        private class CountryName : LinkFlowContainer
        {
            [Resolved(canBeNull: true)]
            private RankingsOverlay rankings { get; set; }

            public CountryName(Country country)
                : base(t => t.Font = OsuFont.GetFont(size: 12))
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;
                TextAnchor = Anchor.CentreLeft;

                if (!string.IsNullOrEmpty(country.FullName))
                    AddLink(country.FullName, () => rankings?.ShowCountry(country));
            }
        }
    }
}
