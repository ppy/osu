// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsHeader : TabControlOverlayHeader<RankingsScope>
    {
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly Bindable<Country> Country = new Bindable<Country>();
        public readonly Bindable<Spotlight> Spotlight = new Bindable<Spotlight>();

        public IEnumerable<Spotlight> Spotlights
        {
            get => spotlightsContainer.Spotlights;
            set => spotlightsContainer.Spotlights = value;
        }

        private SpotlightsContainer spotlightsContainer;

        public RankingsHeader(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
            BackgroundHeight = 0;

            HeaderInfo.Add(new CountryFilter(ColourScheme)
            {
                Country = { BindTarget = Country }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(onCurrentChanged, true);
        }

        private void onCurrentChanged(ValueChangedEvent<RankingsScope> scope)
        {
            if (scope.NewValue == RankingsScope.Spotlights)
            {
                spotlightsContainer.Show();
                return;
            }

            spotlightsContainer.Hide();
        }

        protected override Drawable CreateTitleContent() => new RankingsRulesetSelector(ColourScheme)
        {
            Current = Ruleset,
        };

        protected override ScreenTitle CreateTitle() => new RankingsHeaderTitle
        {
            Scope = { BindTarget = Current }
        };

        protected override Drawable CreateContent() => spotlightsContainer = new SpotlightsContainer(ColourScheme)
        {
            Spotlight = { BindTarget = Spotlight }
        };

        private class RankingsRulesetSelector : OverlayRulesetSelector
        {
            public RankingsRulesetSelector(OverlayColourScheme colourScheme)
                : base(colourScheme)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                SelectTab(TabContainer.FirstOrDefault());
            }
        }

        private class SpotlightsContainer : VisibilityContainer
        {
            private const int height = 150;
            private const int duration = 200;

            public readonly Bindable<Spotlight> Spotlight = new Bindable<Spotlight>();

            public IEnumerable<Spotlight> Spotlights
            {
                get => dropdown.Items;
                set => dropdown.Items = value;
            }

            private readonly OsuDropdown<Spotlight> dropdown;
            private readonly Container content;
            private readonly Box background;
            private readonly OverlayColourScheme colourScheme;

            public SpotlightsContainer(OverlayColourScheme colourScheme)
            {
                this.colourScheme = colourScheme;

                RelativeSizeAxes = Axes.X;
                Child = content = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = height,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        dropdown = new OsuDropdown<Spotlight>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.8f,
                            Current = Spotlight,
                            Margin = new MarginPadding { Top = 5 }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.ForOverlayElement(colourScheme, 0.2f, 0.25f);
            }

            protected override void PopOut()
            {
                this.ResizeHeightTo(0, duration, Easing.OutQuint);
                content.FadeOut(duration, Easing.OutQuint);
            }

            protected override void PopIn()
            {
                this.ResizeHeightTo(height, duration, Easing.OutQuint);
                content.FadeIn(duration, Easing.OutQuint);
            }
        }

        private class RankingsHeaderTitle : ScreenTitle
        {
            public readonly Bindable<RankingsScope> Scope = new Bindable<RankingsScope>();

            public RankingsHeaderTitle()
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
