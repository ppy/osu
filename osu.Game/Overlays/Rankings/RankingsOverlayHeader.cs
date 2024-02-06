// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Bindables;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Overlays.Rankings
{
    public partial class RankingsOverlayHeader : TabControlOverlayHeader<RankingsScope>
    {
        public Bindable<RulesetInfo> Ruleset => rulesetSelector.Current;

        public Bindable<CountryCode> Country => countryFilter.Current;

        private OverlayRulesetSelector rulesetSelector;
        private CountryFilter countryFilter;

        protected override OverlayTitle CreateTitle() => new RankingsTitle();

        protected override Drawable CreateTabControlContent() => rulesetSelector = new OverlayRulesetSelector();

        protected override Drawable CreateContent() => countryFilter = new CountryFilter();

        protected override Drawable CreateBackground() => new OverlayHeaderBackground("Headers/rankings");

        private partial class RankingsTitle : OverlayTitle
        {
            public RankingsTitle()
            {
                Title = PageTitleStrings.MainRankingControllerDefault;
                Description = NamedOverlayComponentStrings.RankingsDescription;
                Icon = OsuIcon.Ranking;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(scope =>
            {
                rulesetSelector.FadeTo(scope.NewValue <= RankingsScope.Country ? 1 : 0, 200, Easing.OutQuint);
            });
        }
    }
}
