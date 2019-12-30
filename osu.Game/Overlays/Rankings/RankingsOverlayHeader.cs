// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsOverlayHeader : TabControlOverlayHeader<RankingsScope>
    {
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly Bindable<Country> Country = new Bindable<Country>();

        private OverlayRulesetSelector rulesetSelector;

        public RankingsOverlayHeader()
        {
            HeaderInfo.Add(new CountryFilter
            {
                Country = { BindTarget = Country }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundHeight = 0;
            TitleBackgroundColour = colours.GreySeafoamDark;
            ControlBackgroundColour = colours.GreySeafoam;
            rulesetSelector.AccentColour = TabControl.AccentColour = colours.Lime;
        }

        protected override Drawable CreateTitleContent() => rulesetSelector = new OverlayRulesetSelector
        {
            Current = Ruleset,
        };

        protected override ScreenTitle CreateTitle() => new RankingsHeaderTitle
        {
            Scope = { BindTarget = Current }
        };

        private class RankingsHeaderTitle : ScreenTitle
        {
            public readonly Bindable<RankingsScope> Scope = new Bindable<RankingsScope>();

            public RankingsHeaderTitle()
            {
                Title = "ranking";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Lime;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Scope.BindValueChanged(scope => Section = scope.NewValue.ToString(), true);
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/rankings");
        }
    }
}
