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
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsApi : OsuTestScene
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly FillFlowContainer usersContainer;
        private GetRankingsPerformanceRequest request;

        public TestSceneRankingsApi()
        {
            Add(usersContainer = new FillFlowContainer
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Full,
                Spacing = new Vector2(10, 5),
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("Get osu performance", () =>
            {
                request?.Cancel();
                request = new GetRankingsPerformanceRequest(new OsuRuleset().RulesetInfo);
                request.Success += updateRankings;
                api.Queue(request);
            });

            AddStep("Get USA osu performance", () =>
            {
                request?.Cancel();
                request = new GetRankingsPerformanceRequest(new OsuRuleset().RulesetInfo, country: "US");
                request.Success += updateRankings;
                api.Queue(request);
            });

            AddStep("Get osu performance page 10", () =>
            {
                request?.Cancel();
                request = new GetRankingsPerformanceRequest(new OsuRuleset().RulesetInfo, 10);
                request.Success += updateRankings;
                api.Queue(request);
            });

            AddStep("Get osu performance page 100", () =>
            {
                request?.Cancel();
                request = new GetRankingsPerformanceRequest(new OsuRuleset().RulesetInfo, 100);
                request.Success += updateRankings;
                api.Queue(request);
            });

            AddStep("Get mania performance", () =>
            {
                request?.Cancel();
                request = new GetRankingsPerformanceRequest(new ManiaRuleset().RulesetInfo);
                request.Success += updateRankings;
                api.Queue(request);
            });

            AddStep("Get taiko performance", () =>
            {
                request?.Cancel();
                request = new GetRankingsPerformanceRequest(new TaikoRuleset().RulesetInfo);
                request.Success += updateRankings;
                api.Queue(request);
            });

            AddStep("Get catch performance", () =>
            {
                request?.Cancel();
                request = new GetRankingsPerformanceRequest(new CatchRuleset().RulesetInfo);
                request.Success += updateRankings;
                api.Queue(request);
            });
        }

        private void updateRankings(List<APIUser> rankings)
        {
            usersContainer.Clear();
            rankings.ForEach(r => usersContainer.Add(new OsuSpriteText
            {
                Text = $"{r.User.Username}"
            }));
        }
    }
}
