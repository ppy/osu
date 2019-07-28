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

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsApi : OsuTestScene
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly FillFlowContainer usersContainer;
        private GetRankingsPerformanceRequest performanceRequest;
        private GetRankingsScoresRequest scoresRequest;

        public TestSceneRankingsApi()
        {
            Add(usersContainer = new FillFlowContainer
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("Get osu performance", () =>
            {
                scoresRequest?.Cancel();
                performanceRequest?.Cancel();
                performanceRequest = new GetRankingsPerformanceRequest(new OsuRuleset().RulesetInfo);
                performanceRequest.Success += updatePerformanceRankings;
                api.Queue(performanceRequest);
            });

            AddStep("Get USA osu performance", () =>
            {
                scoresRequest?.Cancel();
                performanceRequest?.Cancel();
                performanceRequest = new GetRankingsPerformanceRequest(new OsuRuleset().RulesetInfo, country: "US");
                performanceRequest.Success += updatePerformanceRankings;
                api.Queue(performanceRequest);
            });

            AddStep("Get osu performance page 100", () =>
            {
                scoresRequest?.Cancel();
                performanceRequest?.Cancel();
                performanceRequest = new GetRankingsPerformanceRequest(new OsuRuleset().RulesetInfo, 100);
                performanceRequest.Success += updatePerformanceRankings;
                api.Queue(performanceRequest);
            });

            AddStep("Get mania performance", () =>
            {
                scoresRequest?.Cancel();
                performanceRequest?.Cancel();
                performanceRequest = new GetRankingsPerformanceRequest(new ManiaRuleset().RulesetInfo);
                performanceRequest.Success += updatePerformanceRankings;
                api.Queue(performanceRequest);
            });

            AddStep("Get taiko performance", () =>
            {
                scoresRequest?.Cancel();
                performanceRequest?.Cancel();
                performanceRequest = new GetRankingsPerformanceRequest(new TaikoRuleset().RulesetInfo);
                performanceRequest.Success += updatePerformanceRankings;
                api.Queue(performanceRequest);
            });

            AddStep("Get catch performance", () =>
            {
                scoresRequest?.Cancel();
                performanceRequest?.Cancel();
                performanceRequest = new GetRankingsPerformanceRequest(new CatchRuleset().RulesetInfo);
                performanceRequest.Success += updatePerformanceRankings;
                api.Queue(performanceRequest);
            });

            AddStep("Get mania scores for BY", () =>
            {
                scoresRequest?.Cancel();
                performanceRequest?.Cancel();
                scoresRequest = new GetRankingsScoresRequest(new ManiaRuleset().RulesetInfo, country: "BY");
                scoresRequest.Success += updateScoreRankings;
                api.Queue(scoresRequest);
            });
        }

        private void updatePerformanceRankings(List<APIUserRankings> rankings)
        {
            usersContainer.Clear();
            rankings.ForEach(r => usersContainer.Add(new OsuSpriteText
            {
                Text = $"{r.User.Username}, Accuracy: {r.Accuracy}, Play Count: {r.PlayCount}, Performance: {r.PP}, SS: {r.GradesCount.SS}, S: {r.GradesCount.S}, A: {r.GradesCount.A}"
            }));
        }

        private void updateScoreRankings(List<APIUserRankings> rankings)
        {
            usersContainer.Clear();
            rankings.ForEach(r => usersContainer.Add(new OsuSpriteText
            {
                Text = $"{r.User.Username}, Accuracy: {r.Accuracy}, Play Count: {r.PlayCount}, Total Score: {r.TotalScore}, Ranked Score: {r.RankedScore}, SS: {r.GradesCount.SS}, S: {r.GradesCount.S}, A: {r.GradesCount.A}"
            }));
        }
    }
}
