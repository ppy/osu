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
using osu.Game.Online.API;

namespace osu.Game.Overlays
{
    public class RankingsOverlay : FullscreenOverlay
    {
        public const float CONTENT_X_MARGIN = 50;

        protected readonly Bindable<Country> Country = new Bindable<Country>();

        protected Bindable<RankingsScope> Scope => header.Current;

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; }

        private readonly Container contentContainer;
        private readonly LoadingLayer loading;
        private readonly Box background;
        private readonly RankingsOverlayHeader header;
        private readonly OverlayScrollContainer scrollFlow;

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
                scrollFlow = new OverlayScrollContainer
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
                            contentContainer = new Container
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

                Scheduler.AddOnce(selectDisplayToLoad);
            });
        }

        private bool displayUpdateRequired = true;

        protected override void PopIn()
        {
            base.PopIn();

            // We don't want to create a new display on every call, only when exiting from fully closed state.
            if (displayUpdateRequired)
            {
                header.Current.TriggerChange();
                displayUpdateRequired = false;
            }
        }

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            loadDisplayAsync(Empty());
            displayUpdateRequired = true;
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

        private void selectDisplayToLoad()
        {
            cancellationToken?.Cancel();
            spotlightsRequest?.Cancel();

            loading.Show();

            if (!API.IsLoggedIn)
            {
                loadDisplayAsync(Empty());
                return;
            }

            if (Scope.Value == RankingsScope.Spotlights)
            {
                loadSpotlightsDisplay();
                return;
            }

            loadDisplayAsync(selectDisplay());
        }

        private void loadSpotlightsDisplay()
        {
            spotlightsRequest = new GetSpotlightsRequest();
            spotlightsRequest.Success += response => Schedule(() =>
            {
                var display = new SpotlightsRankingsDisplay
                {
                    Current = ruleset,
                    StartLoading = loading.Show,
                    FinishLoading = loading.Hide,
                    Spotlights = response.Spotlights
                };
                loadDisplayAsync(display);
            });
            API.Queue(spotlightsRequest);
        }

        private void loadDisplayAsync(Drawable display)
        {
            scrollFlow.ScrollToStart();

            LoadComponentAsync(display, loaded =>
            {
                contentContainer.Child = loaded;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private Drawable selectDisplay()
        {
            switch (Scope.Value)
            {
                case RankingsScope.Country:
                    return new CountryRankingsDisplay
                    {
                        Current = ruleset,
                        StartLoading = loading.Show,
                        FinishLoading = loading.Hide
                    };

                case RankingsScope.Performance:
                    return new PerformanceRankingsDisplay
                    {
                        Country = { BindTarget = Country },
                        Current = ruleset,
                        StartLoading = loading.Show,
                        FinishLoading = loading.Hide
                    };

                case RankingsScope.Score:
                    return new ScoreRankingsDisplay
                    {
                        Current = ruleset,
                        StartLoading = loading.Show,
                        FinishLoading = loading.Hide
                    };
            }

            throw new NotImplementedException($"Display for {Scope.Value} is not implemented.");
        }

        public override void APIStateChanged(IAPIProvider api, APIState state)
        {
            if (State.Value == Visibility.Hidden)
                return;

            Scope.TriggerChange();
        }

        protected override void Dispose(bool isDisposing)
        {
            spotlightsRequest?.Cancel();
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
