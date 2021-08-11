// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePlayerScoreSubmission : PlayerTestScene
    {
        protected override bool AllowFail => allowFail;

        private bool allowFail;

        private Func<RulesetInfo, IBeatmap> createCustomBeatmap;
        private Func<Ruleset> createCustomRuleset;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        protected override bool HasCustomSteps => true;

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(false);

        protected override Ruleset CreatePlayerRuleset() => createCustomRuleset?.Invoke() ?? new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => createCustomBeatmap?.Invoke(ruleset) ?? createTestBeatmap(ruleset);

        private IBeatmap createTestBeatmap(RulesetInfo ruleset)
        {
            var beatmap = (TestBeatmap)base.CreateBeatmap(ruleset);

            beatmap.HitObjects = beatmap.HitObjects.Take(10).ToList();

            return beatmap;
        }

        [Test]
        public void TestNoSubmissionOnResultsWithNoToken()
        {
            prepareTokenResponse(false);

            createPlayerTest();

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            addFakeHit();

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);

            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestSubmissionOnResults()
        {
            prepareTokenResponse(true);

            createPlayerTest();

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            addFakeHit();

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);
            AddAssert("ensure passing submission", () => Player.SubmittedScore?.ScoreInfo.Passed == true);
        }

        [Test]
        public void TestNoSubmissionOnExitWithNoToken()
        {
            prepareTokenResponse(false);

            createPlayerTest();

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            addFakeHit();

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestNoSubmissionOnEmptyFail()
        {
            prepareTokenResponse(true);

            createPlayerTest(true);

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddStep("exit", () => Player.Exit());

            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestSubmissionOnFail()
        {
            prepareTokenResponse(true);

            createPlayerTest(true);

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            addFakeHit();

            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddStep("exit", () => Player.Exit());

            AddAssert("ensure failing submission", () => Player.SubmittedScore?.ScoreInfo.Passed == false);
        }

        [Test]
        public void TestNoSubmissionOnEmptyExit()
        {
            prepareTokenResponse(true);

            createPlayerTest();

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestSubmissionOnExit()
        {
            prepareTokenResponse(true);

            createPlayerTest();

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            addFakeHit();

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure failing submission", () => Player.SubmittedScore?.ScoreInfo.Passed == false);
        }

        [Test]
        public void TestNoSubmissionOnLocalBeatmap()
        {
            prepareTokenResponse(true);

            createPlayerTest(false, r =>
            {
                var beatmap = createTestBeatmap(r);
                beatmap.BeatmapInfo.OnlineBeatmapID = null;
                return beatmap;
            });

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            addFakeHit();

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestNoSubmissionOnCustomRuleset()
        {
            prepareTokenResponse(true);

            createPlayerTest(false, createRuleset: () => new OsuRuleset { RulesetInfo = { ID = 10 } });

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            addFakeHit();

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        private void createPlayerTest(bool allowFail = false, Func<RulesetInfo, IBeatmap> createBeatmap = null, Func<Ruleset> createRuleset = null)
        {
            CreateTest(() => AddStep("set up requirements", () =>
            {
                this.allowFail = allowFail;
                createCustomBeatmap = createBeatmap;
                createCustomRuleset = createRuleset;
            }));
        }

        private void prepareTokenResponse(bool validToken)
        {
            AddStep("Prepare test API", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    switch (request)
                    {
                        case CreateSoloScoreRequest tokenRequest:
                            if (validToken)
                                tokenRequest.TriggerSuccess(new APIScoreToken { ID = 1234 });
                            else
                                tokenRequest.TriggerFailure(new APIException("something went wrong!", null));
                            return true;
                    }

                    return false;
                };
            });
        }

        private void addFakeHit()
        {
            AddUntilStep("wait for first result", () => Player.Results.Count > 0);

            AddStep("force successfuly hit", () =>
            {
                Player.ScoreProcessor.RevertResult(Player.Results.First());
                Player.ScoreProcessor.ApplyResult(new OsuJudgementResult(Beatmap.Value.Beatmap.HitObjects.First(), new OsuJudgement())
                {
                    Type = HitResult.Great,
                });
            });
        }
    }
}
