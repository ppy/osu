// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Users;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsOverlayHeader : TabControlOverlayHeader<RankingsScope>
    {
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly Bindable<APISpotlight> Spotlight = new Bindable<APISpotlight>();
        public readonly Bindable<Country> Country = new Bindable<Country>();

        public IEnumerable<APISpotlight> Spotlights
        {
            get => spotlightSelector.Spotlights;
            set => spotlightSelector.Spotlights = value;
        }

        protected override ScreenTitle CreateTitle() => new RankingsTitle
        {
            Scope = { BindTarget = Current }
        };

        protected override Drawable CreateTitleContent() => new OverlayRulesetSelector
        {
            Current = Ruleset
        };

        private SpotlightSelector spotlightSelector;

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Children = new Drawable[]
            {
                new CountryFilter
                {
                    Current = Country
                },
                spotlightSelector = new SpotlightSelector
                {
                    Current = { BindTarget = Spotlight }
                }
            }
        };

        protected override void LoadComplete()
        {
            Current.BindValueChanged(onCurrentChanged, true);
            base.LoadComplete();
        }

        private void onCurrentChanged(ValueChangedEvent<RankingsScope> scope) =>
            spotlightSelector.FadeTo(scope.NewValue == RankingsScope.Spotlights ? 1 : 0, 200, Easing.OutQuint);

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
