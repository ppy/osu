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
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using System.Threading;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Rankings.Tables;

namespace osu.Game.Overlays
{
    public class RankingsOverlay : FullscreenOverlay
    {
        private readonly Bindable<Country> country = new Bindable<Country>();
        private readonly Bindable<RankingsScope> scope = new Bindable<RankingsScope>();
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private readonly BasicScrollContainer scrollFlow;
        private readonly Box background;
        private readonly Container contentPlaceholder;
        private readonly DimmedLoadingLayer loading;

        private APIRequest request;
        private CancellationTokenSource cancellationToken;

        [Resolved]
        private IAPIProvider api { get; set; }

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
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    contentPlaceholder = new Container
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

            country.Value = requested;
        }

        private void redraw()
        {
            scrollFlow.ScrollToStart();

            loading.Show();

            cancellationToken?.Cancel();
            request?.Cancel();

            cancellationToken = new CancellationTokenSource();

            switch (scope.Value)
            {
                default:
                    contentPlaceholder.Clear();
                    loading.Hide();
                    return;

                case RankingsScope.Performance:
                    createPerformanceTable();
                    return;

                case RankingsScope.Country:
                    createCountryTable();
                    return;

                case RankingsScope.Score:
                    createScoreTable();
                    return;
            }
        }

        private void createCountryTable()
        {
            request = new GetCountryRankingsRequest(ruleset.Value);
            ((GetCountryRankingsRequest)request).Success += rankings => Schedule(() =>
            {
                var table = new CountriesTable(1, rankings.Countries);
                loadTable(table);
            });

            api.Queue(request);
        }

        private void createPerformanceTable()
        {
            request = new GetUserRankingsRequest(ruleset.Value, country: country.Value?.FlagName);
            ((GetUserRankingsRequest)request).Success += rankings => Schedule(() =>
            {
                var table = new PerformanceTable(1, rankings.Users);
                loadTable(table);
            });

            api.Queue(request);
        }

        private void createScoreTable()
        {
            request = new GetUserRankingsRequest(ruleset.Value, UserRankingsType.Score);
            ((GetUserRankingsRequest)request).Success += rankings => Schedule(() =>
            {
                var table = new ScoresTable(1, rankings.Users);
                loadTable(table);
            });

            api.Queue(request);
        }

        private void loadTable(Drawable table)
        {
            LoadComponentAsync(table, t =>
            {
                contentPlaceholder.Clear();
                contentPlaceholder.Add(t);
                loading.Hide();
            }, cancellationToken.Token);
        }
    }
}
