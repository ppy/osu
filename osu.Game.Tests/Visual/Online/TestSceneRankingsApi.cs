// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Osu;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Catch;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsApi : OsuTestScene
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly FillFlowContainer flow;
        private readonly DimmedLoadingLayer loading;

        private GetUserRankingsRequest userRequest;
        private GetCountryRankingsRequest countryRequest;

        public TestSceneRankingsApi()
        {
            AddRange(new Drawable[]
            {
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = flow = new FillFlowContainer
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                    }
                },
                loading = new DimmedLoadingLayer
                {
                    Alpha = 0,
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("Get osu performance", () =>
                updateUserRequest(new GetUserRankingsRequest(new OsuRuleset().RulesetInfo), updatePerformanceRankings));

            AddStep("Get USA osu performance", () =>
                updateUserRequest(new GetUserRankingsRequest(new OsuRuleset().RulesetInfo, country: "US"), updatePerformanceRankings));

            AddStep("Get osu performance page 100", () =>
                updateUserRequest(new GetUserRankingsRequest(new OsuRuleset().RulesetInfo, page: 100), updatePerformanceRankings));

            AddStep("Get mania performance", () =>
                updateUserRequest(new GetUserRankingsRequest(new ManiaRuleset().RulesetInfo), updatePerformanceRankings));

            AddStep("Get taiko performance", () =>
                updateUserRequest(new GetUserRankingsRequest(new TaikoRuleset().RulesetInfo), updatePerformanceRankings));

            AddStep("Get catch performance", () =>
                updateUserRequest(new GetUserRankingsRequest(new CatchRuleset().RulesetInfo), updatePerformanceRankings));

            AddStep("Get mania scores for BY", () =>
                updateUserRequest(new GetUserRankingsRequest(new ManiaRuleset().RulesetInfo, UserRankingsType.Score, country: "BY"), updateScoreRankings));

            AddStep("Get osu country rankings", () =>
                updateCountryRequest(new GetCountryRankingsRequest(new OsuRuleset().RulesetInfo), updateCountryRankings));
        }

        private void updateUserRequest(GetUserRankingsRequest newRequest, APISuccessHandler<List<APIUserRankings>> onSuccess)
        {
            loading.Show();
            userRequest?.Cancel();
            countryRequest?.Cancel();
            userRequest = newRequest;
            userRequest.Success += onSuccess;
            api.Queue(userRequest);
        }

        private void updateCountryRequest(GetCountryRankingsRequest newRequest, APISuccessHandler<List<APICountryRankings>> onSuccess)
        {
            loading.Show();
            userRequest?.Cancel();
            countryRequest?.Cancel();
            countryRequest = newRequest;
            countryRequest.Success += onSuccess;
            api.Queue(countryRequest);
        }

        private void updatePerformanceRankings(List<APIUserRankings> rankings)
        {
            flow.Clear();

            rankings.ForEach(r => flow.Add(new OsuSpriteText
            {
                Text = $"{r.User.Username}, Accuracy: {r.Accuracy}, Play Count: {r.PlayCount}, Performance: {r.PP}, SS: {r.GradesCount.SS}, S: {r.GradesCount.S}, A: {r.GradesCount.A}"
            }));

            loading.Hide();
        }

        private void updateScoreRankings(List<APIUserRankings> rankings)
        {
            flow.Clear();

            rankings.ForEach(r => flow.Add(new OsuSpriteText
            {
                Text = $"{r.User.Username}, Accuracy: {r.Accuracy}, Play Count: {r.PlayCount}, Total Score: {r.TotalScore}, Ranked Score: {r.RankedScore}, SS: {r.GradesCount.SS}, S: {r.GradesCount.S}, A: {r.GradesCount.A}"
            }));

            loading.Hide();
        }

        private void updateCountryRankings(List<APICountryRankings> rankings)
        {
            flow.Clear();

            rankings.ForEach(r => flow.Add(new OsuSpriteText
            {
                Text = $"{r.FlagName}, Active Users: {r.ActiveUsers}, Play Count: {r.PlayCount}, Ranked Score: {r.RankedScore}, Performance: {r.Performance}"
            }));

            loading.Hide();
        }
    }
}
