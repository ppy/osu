// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Users;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsOverlayHeader : TabControlOverlayHeader<RankingsScope>
    {
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly Bindable<Country> Country = new Bindable<Country>();

        public IEnumerable<Spotlight> Spotlights { get; set; }

        protected override ScreenTitle CreateTitle() => new RankingsTitle
        {
            Scope = { BindTarget = Current }
        };

        protected override Drawable CreateTitleContent() => new OverlayRulesetSelector
        {
            Current = Ruleset
        };

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new CountryFilter
                {
                    Current = Country
                }
            }
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
}
