// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays.Rankings;
using osu.Game.Users;
using osu.Game.Rulesets;
using System.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Rankings.Displays;
using System;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays
{
    public class RankingsOverlay : FullscreenOverlay
    {
        protected readonly Bindable<Country> Country = new Bindable<Country>();

        protected Bindable<RankingsScope> Scope => header.Current;

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; }

        private readonly Container<RankingsDisplay> contentContainer;
        private readonly LoadingLayer loading;
        private readonly Box background;
        private readonly RankingsOverlayHeader header;

        private GetSpotlightsRequest spotlightsRequest;
        private CancellationTokenSource cancellationToken;

        public RankingsOverlay()
            : base(OverlayColourScheme.Green)
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new OverlayScrollContainer
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
                            header = new RankingsOverlayHeader
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Depth = -float.MaxValue
                            },
                            contentContainer = new Container<RankingsDisplay>
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                            }
                        }
                    }
                },
                loading = new LoadingLayer()
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = ColourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            header.Ruleset.BindTo(ruleset);

            Country.BindValueChanged(country =>
            {
                if (country.NewValue != null)
                    Scope.Value = RankingsScope.Performance;
            });

            Scope.BindValueChanged(scope =>
            {
                if (scope.NewValue != RankingsScope.Performance)
                    Country.Value = null;

                Scheduler.AddOnce(loadNewDisplay);
            }, true);
        }

        private void loadNewDisplay()
        {
            cancellationToken?.Cancel();
            spotlightsRequest?.Cancel();

            loading.Show();

            var display = selectDisplay().With(d =>
            {
                d.Current = ruleset;
                d.StartLoading = loading.Show;
                d.FinishLoading = loading.Hide;
            });

            if (display is SpotlightsRankingsDisplay)
            {
                loadSpotlightsDisplay((SpotlightsRankingsDisplay)display);
                return;
            }

            loadDisplayAsync(display);
        }

        private void loadSpotlightsDisplay(SpotlightsRankingsDisplay display)
        {
            spotlightsRequest = new GetSpotlightsRequest();
            spotlightsRequest.Success += response => Schedule(() =>
            {
                display.Spotlights = response.Spotlights;
                loadDisplayAsync(display);
            });
            API.Queue(spotlightsRequest);
        }

        private void loadDisplayAsync(RankingsDisplay display)
        {
            LoadComponentAsync(display, loaded =>
            {
                contentContainer.Child = loaded;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private RankingsDisplay selectDisplay()
        {
            switch (Scope.Value)
            {
                case RankingsScope.Country:
                    return new CountryRankingsDisplay();

                case RankingsScope.Performance:
                    return new PerformanceRankingsDisplay
                    {
                        Country = { BindTarget = Country }
                    };

                case RankingsScope.Score:
                    return new ScoreRankingsDisplay();

                case RankingsScope.Spotlights:
                    return new SpotlightsRankingsDisplay();
            }

            throw new NotImplementedException($"Display for {Scope.Value} is not implemented.");
        }

        public void ShowCountry(Country requested)
        {
            if (requested == null)
                return;

            Country.Value = requested;

            Show();
        }

        public void ShowSpotlights()
        {
            Scope.Value = RankingsScope.Spotlights;
            Show();
        }

        protected override void Dispose(bool isDisposing)
        {
            spotlightsRequest?.Cancel();
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
