// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Rankings.Tables;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Graphics.UserInterface;
using System.Threading;
using osu.Game.Online.API;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Catch;
using osu.Framework.Allocation;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsTables : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(PerformanceTable),
            typeof(ScoresTable),
            typeof(CountriesTable),
            typeof(TableRowBackground),
            typeof(UserBasedTable),
            typeof(RankingsTable<>)
        };

        [Resolved]
        private IAPIProvider api { get; set; }

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        private readonly BasicScrollContainer scrollFlow;
        private readonly DimmedLoadingLayer loading;
        private CancellationTokenSource cancellationToken;
        private APIRequest request;

        public TestSceneRankingsTables()
        {
            Children = new Drawable[]
            {
                scrollFlow = new BasicScrollContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.8f,
                },
                loading = new DimmedLoadingLayer(),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("Osu performance", () => createPerformanceTable(new OsuRuleset().RulesetInfo, null));
            AddStep("Mania scores", () => createScoreTable(new ManiaRuleset().RulesetInfo));
            AddStep("Taiko country scores", () => createCountryTable(new TaikoRuleset().RulesetInfo));
            AddStep("Catch US performance page 10", () => createPerformanceTable(new CatchRuleset().RulesetInfo, "US", 10));
            AddStep("Osu spotlight table (chart 271)", () => createSpotlightTable(new OsuRuleset().RulesetInfo, 271));
        }

        private void createCountryTable(RulesetInfo ruleset, int page = 1)
        {
            onLoadStarted();

            request = new GetCountryRankingsRequest(ruleset, page);
            ((GetCountryRankingsRequest)request).Success += rankings => Schedule(() =>
            {
                var table = new CountriesTable(page, rankings.Countries);
                loadTable(table);
            });

            api.Queue(request);
        }

        private void createPerformanceTable(RulesetInfo ruleset, string country, int page = 1)
        {
            onLoadStarted();

            request = new GetUserRankingsRequest(ruleset, country: country, page: page);
            ((GetUserRankingsRequest)request).Success += rankings => Schedule(() =>
            {
                var table = new PerformanceTable(page, rankings.Users);
                loadTable(table);
            });

            api.Queue(request);
        }

        private void createScoreTable(RulesetInfo ruleset, int page = 1)
        {
            onLoadStarted();

            request = new GetUserRankingsRequest(ruleset, UserRankingsType.Score, page);
            ((GetUserRankingsRequest)request).Success += rankings => Schedule(() =>
            {
                var table = new ScoresTable(page, rankings.Users);
                loadTable(table);
            });

            api.Queue(request);
        }

        private void createSpotlightTable(RulesetInfo ruleset, int spotlight)
        {
            onLoadStarted();

            request = new GetSpotlightRankingsRequest(ruleset, spotlight);
            ((GetSpotlightRankingsRequest)request).Success += rankings => Schedule(() =>
            {
                var table = new ScoresTable(1, rankings.Users);
                loadTable(table);
            });

            api.Queue(request);
        }

        private void onLoadStarted()
        {
            loading.Show();
            request?.Cancel();
            cancellationToken?.Cancel();
            cancellationToken = new CancellationTokenSource();
        }

        private void loadTable(Drawable table)
        {
            LoadComponentAsync(table, t =>
            {
                scrollFlow.Clear();
                scrollFlow.Add(t);
                loading.Hide();
            }, cancellationToken.Token);
        }
    }
}
