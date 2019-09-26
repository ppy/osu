// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays.Rankings;
using osu.Game.Users;
using osu.Game.Rulesets;

namespace osu.Game.Overlays
{
    public class RankingsOverlay : FullscreenOverlay
    {
        private readonly Bindable<Country> country = new Bindable<Country>();
        private readonly Bindable<RankingsScope> scope = new Bindable<RankingsScope>();
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private readonly BasicScrollContainer scrollFlow;
        private readonly Box background;

        public RankingsOverlay()
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                scrollFlow = new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new RankingsHeader
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Country = { BindTarget = country },
                                Scope = { BindTarget = scope },
                                Ruleset = { BindTarget = ruleset }
                            },
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Waves.FirstWaveColour = colour.Green;
            Waves.SecondWaveColour = colour.GreenLight;
            Waves.ThirdWaveColour = colour.GreenDark;
            Waves.FourthWaveColour = colour.GreenDarker;

            background.Colour = OsuColour.Gray(0.1f);
        }

        protected override void LoadComplete()
        {
            country.BindValueChanged(_ => redraw(), true);
            scope.BindValueChanged(_ => redraw(), true);
            ruleset.BindValueChanged(_ => redraw(), true);
            base.LoadComplete();
        }

        public void ShowCountry(Country requested)
        {
            if (requested == null)
                return;

            Show();

            if (country.Value?.FlagName == requested.FlagName)
                return;

            country.Value = requested;
        }

        private void redraw()
        {
            scrollFlow.ScrollToStart();
        }
    }
}
