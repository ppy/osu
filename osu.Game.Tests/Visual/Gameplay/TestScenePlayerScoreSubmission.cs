// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePlayerScoreSubmission : OsuPlayerTestScene
    {
        protected override bool AllowFail => allowFail;

        private bool allowFail;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        protected override bool HasCustomSteps => true;

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(false);

        [Test]
        public void TestNoSubmissionOnResultsWithNoToken()
        {
            prepareTokenResponse(false);

            CreateTest(() => allowFail = false);

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);

            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestSubmissionOnResults()
        {
            prepareTokenResponse(true);

            CreateTest(() => allowFail = false);

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

            CreateTest(() => allowFail = false);

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestNoSubmissionOnEmptyFail()
        {
            prepareTokenResponse(true);

            CreateTest(() => allowFail = true);

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddStep("exit", () => Player.Exit());

            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestSubmissionOnFail()
        {
            prepareTokenResponse(true);

            CreateTest(() => allowFail = true);

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

            CreateTest(() => allowFail = false);

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestSubmissionOnExit()
        {
            prepareTokenResponse(true);

            CreateTest(() => allowFail = false);

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            addFakeHit();

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure failing submission", () => Player.SubmittedScore?.ScoreInfo.Passed == false);
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
    }
}
