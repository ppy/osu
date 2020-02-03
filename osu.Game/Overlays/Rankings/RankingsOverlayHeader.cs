// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Users;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsOverlayHeader : TabControlOverlayHeader<RankingsScope>
    {
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly Bindable<Spotlight> Spotlight = new Bindable<Spotlight>();
        public readonly Bindable<Country> Country = new Bindable<Country>();

        public IEnumerable<Spotlight> Spotlights
        {
            get => spotlightsContainer.Spotlights;
            set => spotlightsContainer.Spotlights = value;
        }

        protected override ScreenTitle CreateTitle() => new RankingsTitle
        {
            Scope = { BindTarget = Current }
        };

        protected override Drawable CreateTitleContent() => new OverlayRulesetSelector
        {
            Current = Ruleset
        };

        private SpotlightsContainer spotlightsContainer;

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
                spotlightsContainer = new SpotlightsContainer
                {
                    Spotlight = { BindTarget = Spotlight }
                }
            }
        };

        protected override void LoadComplete()
        {
            Current.BindValueChanged(onCurrentChanged, true);
            base.LoadComplete();
        }

        private void onCurrentChanged(ValueChangedEvent<RankingsScope> scope) =>
            spotlightsContainer.FadeTo(scope.NewValue == RankingsScope.Spotlights ? 1 : 0, 200, Easing.OutQuint);

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

        private class SpotlightsContainer : CompositeDrawable
        {
            public readonly Bindable<Spotlight> Spotlight = new Bindable<Spotlight>();

            public IEnumerable<Spotlight> Spotlights
            {
                get => dropdown.Items;
                set => dropdown.Items = value;
            }

            private readonly OsuDropdown<Spotlight> dropdown;
            private readonly Box background;

            public SpotlightsContainer()
            {
                Height = 100;
                RelativeSizeAxes = Axes.X;
                InternalChildren = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    dropdown = new OsuDropdown<Spotlight>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Width = 0.8f,
                        Current = Spotlight,
                        Y = 20,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                background.Colour = colourProvider.Dark3;
            }
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
