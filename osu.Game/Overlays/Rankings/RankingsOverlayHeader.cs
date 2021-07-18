// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Rulesets;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsOverlayHeader : TabControlOverlayHeader<RankingsScope>
    {
        public Bindable<RulesetInfo> Ruleset => rulesetSelector.Current;

        public Bindable<Country> Country => countryFilter.Current;

        private OverlayRulesetSelector rulesetSelector;
        private CountryFilter countryFilter;

        protected override OverlayTitle CreateTitle() => new RankingsTitle();

        protected override Drawable CreateTitleContent() => rulesetSelector = new OverlayRulesetSelector();

        protected override Drawable CreateContent() => countryFilter = new CountryFilter();

        protected override Drawable CreateBackground() => new OverlayHeaderBackground("Headers/rankings");

        private class RankingsTitle : OverlayTitle
        {
            public RankingsTitle()
            {
                Title = LayoutStrings.MenuRankingsDefault;
                Description = NamedOverlayComponentStrings.RankingsDescription;
                IconTexture = "Icons/Hexacons/rankings";
            }
        }
    }

    [LocalisableEnum(typeof(RankingsScopeEnumLocalisationMapper))]
    public enum RankingsScope
    {
        Performance,
        Spotlights,
        Score,
        Country
    }

    public class RankingsScopeEnumLocalisationMapper : EnumLocalisationMapper<RankingsScope>
    {
        public override LocalisableString Map(RankingsScope value)
        {
            switch (value)
            {
                case RankingsScope.Performance:
                    return LayoutStrings.MenuRankingsIndex;

                case RankingsScope.Spotlights:
                    return LayoutStrings.MenuRankingsCharts;

                case RankingsScope.Score:
                    return LayoutStrings.MenuRankingsScore;

                case RankingsScope.Country:
                    return LayoutStrings.MenuRankingsCountry;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
