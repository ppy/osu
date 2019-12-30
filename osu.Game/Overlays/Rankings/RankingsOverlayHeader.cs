// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsOverlayHeader : OverlayHeader
    {
        public readonly Bindable<RankingsScope> Scope = new Bindable<RankingsScope>();
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        private OverlayRulesetSelector rulesetSelector;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundHeight = 0;
            TitleBackgroundColour = colours.GreySeafoamDark;
            rulesetSelector.AccentColour = colours.Lime;
        }

        protected override Drawable CreateTitleContent() => rulesetSelector = new OverlayRulesetSelector
        {
            Current = Ruleset,
        };

        protected override ScreenTitle CreateTitle() => new RankingsHeaderTitle
        {
            Scope = { BindTarget = Scope }
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
