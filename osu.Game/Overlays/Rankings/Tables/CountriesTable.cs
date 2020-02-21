// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using osu.Game.Users;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Graphics.Containers;

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

        protected override Drawable CreateFlagContent(CountryStatistics item) => new CountryName(item.Country);

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

        private class CountryName : OsuHoverContainer
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { text };

            [Resolved(canBeNull: true)]
            private RankingsOverlay rankings { get; set; }

            private readonly OsuSpriteText text;
            private readonly Country country;

            public CountryName(Country country)
            {
                this.country = country;

                AutoSizeAxes = Axes.Both;
                Add(text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 12),
                    Text = country.FullName ?? string.Empty,
                });
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Light2;
                HoverColour = colourProvider.Content2;

                Action = () => rankings?.ShowCountry(country);
            }
        }
    }
}
