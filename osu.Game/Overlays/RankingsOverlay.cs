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
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using System.Threading;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Rankings.Tables;

namespace osu.Game.Overlays
{
    public class RankingsOverlay : FullscreenOverlay
    {
        protected readonly Bindable<Country> Country = new Bindable<Country>();
        protected readonly Bindable<RankingsScope> Scope = new Bindable<RankingsScope>();
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private readonly BasicScrollContainer scrollFlow;
        private readonly Container tableContainer;
        private readonly DimmedLoadingLayer loading;
        private readonly Box background;

        private APIRequest lastRequest;
        private CancellationTokenSource cancellationToken;

        [Resolved]
        private IAPIProvider api { get; set; }

        public RankingsOverlay()
            : base(OverlayColourScheme.Green)
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
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
                            new RankingsOverlayHeader
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Depth = -float.MaxValue,
                                Country = { BindTarget = Country },
                                Current = { BindTarget = Scope },
                                Ruleset = { BindTarget = ruleset }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    tableContainer = new Container
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        Margin = new MarginPadding { Vertical = 10 }
                                    },
                                    loading = new DimmedLoadingLayer(),
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = ColourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            Country.BindValueChanged(_ =>
            {
                // if a country is requested, force performance scope.
                if (Country.Value != null)
                    Scope.Value = RankingsScope.Performance;

                Scheduler.AddOnce(loadNewContent);
            }, true);

            Scope.BindValueChanged(_ =>
            {
                // country filtering is only valid for performance scope.
                if (Scope.Value != RankingsScope.Performance)
                    Country.Value = null;

                Scheduler.AddOnce(loadNewContent);
            }, true);

            ruleset.BindValueChanged(_ => Scheduler.AddOnce(loadNewContent), true);

            base.LoadComplete();
        }

        public void ShowCountry(Country requested)
        {
            if (requested == null)
                return;

            Show();

            Country.Value = requested;
        }

        private void loadNewContent()
        {
            loading.Show();

            cancellationToken?.Cancel();
            lastRequest?.Cancel();

            var request = createScopedRequest();
            lastRequest = request;

            if (request == null)
            {
                loadTable(null);
                return;
            }

            request.Success += () => loadTable(createTableFromResponse(request));
            request.Failure += _ => loadTable(null);

            api.Queue(request);
        }

        private APIRequest createScopedRequest()
        {
            switch (Scope.Value)
            {
                case RankingsScope.Performance:
                    return new GetUserRankingsRequest(ruleset.Value, country: Country.Value?.FlagName);

                case RankingsScope.Country:
                    return new GetCountryRankingsRequest(ruleset.Value);

                case RankingsScope.Score:
                    return new GetUserRankingsRequest(ruleset.Value, UserRankingsType.Score);
            }

            return null;
        }

        private Drawable createTableFromResponse(APIRequest request)
        {
            switch (request)
            {
                case GetUserRankingsRequest userRequest:
                    switch (userRequest.Type)
                    {
                        case UserRankingsType.Performance:
                            return new PerformanceTable(1, userRequest.Result.Users);

                        case UserRankingsType.Score:
                            return new ScoresTable(1, userRequest.Result.Users);
                    }

                    return null;

                case GetCountryRankingsRequest countryRequest:
                    return new CountriesTable(1, countryRequest.Result.Countries);
            }

            return null;
        }

        private void loadTable(Drawable table)
        {
            scrollFlow.ScrollToStart();

            if (table == null)
            {
                tableContainer.Clear();
                loading.Hide();
                return;
            }

            LoadComponentAsync(table, t =>
            {
                loading.Hide();
                tableContainer.Child = table;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }
    }
}
