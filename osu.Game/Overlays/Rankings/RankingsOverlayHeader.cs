// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsOverlayHeader : TabControlOverlayHeader<RankingsScope>
    {
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly Bindable<Country> Country = new Bindable<Country>();

        protected override ScreenTitle CreateTitle() => new RankingsTitle
        {
            Scope = { BindTarget = Current }
        };

        protected override Drawable CreateTitleContent() => new OverlayRulesetSelector
        {
            Current = Ruleset
        };

        protected override Drawable CreateContent() => new CountryFilter
        {
            Current = Country
        };

        private class RankingsTitle : ScreenTitle
        {
            public readonly Bindable<RankingsScope> Scope = new Bindable<RankingsScope>();

            public RankingsTitle()
            {
                Title = "ranking";
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Scope.BindValueChanged(scope => Section = scope.NewValue.ToString().ToLowerInvariant(), true);
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/rankings");
        }
    }

    public enum RankingsScope
    {
        Performance,
        Spotlights,
        Score,
        Country
    }
}
