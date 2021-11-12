// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
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

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new NonImportingPlayer(false);

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
        public void TestSubmissionForDifferentRuleset()
        {
            prepareTokenResponse(true);

            createPlayerTest(createRuleset: () => new TaikoRuleset());

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            addFakeHit();

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);
            AddAssert("ensure passing submission", () => Player.SubmittedScore?.ScoreInfo.Passed == true);
            AddAssert("submitted score has correct ruleset ID", () => Player.SubmittedScore?.ScoreInfo.RulesetID == new TaikoRuleset().RulesetInfo.ID);
        }

        [Test]
        public void TestSubmissionForConvertedBeatmap()
        {
            prepareTokenResponse(true);

            createPlayerTest(createRuleset: () => new ManiaRuleset(), createBeatmap: _ => createTestBeatmap(new OsuRuleset().RulesetInfo));

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            addFakeHit();

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);
            AddAssert("ensure passing submission", () => Player.SubmittedScore?.ScoreInfo.Passed == true);
            AddAssert("submitted score has correct ruleset ID", () => Player.SubmittedScore?.ScoreInfo.RulesetID == new ManiaRuleset().RulesetInfo.ID);
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
                beatmap.BeatmapInfo.OnlineID = null;
                return beatmap;
            });

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            addFakeHit();

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [TestCase(null)]
        [TestCase(10)]
        public void TestNoSubmissionOnCustomRuleset(int? rulesetId)
        {
            prepareTokenResponse(true);

            createPlayerTest(false, createRuleset: () => new OsuRuleset { RulesetInfo = { ID = rulesetId } });

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

        private class NonImportingPlayer : TestPlayer
        {
            public NonImportingPlayer(bool allowPause = true, bool showResults = true, bool pauseOnFocusLost = false)
                : base(allowPause, showResults, pauseOnFocusLost)
            {
            }

            protected override Task ImportScore(Score score)
            {
                // It was discovered that Score members could sometimes be half-populated.
                // In particular, the RulesetID property could be set to 0 even on non-osu! maps.
                // We want to test that the state of that property is consistent in this test.
                // EF makes this impossible.
                //
                // First off, because of the EF navigational property-explicit foreign key field duality,
                // it can happen that - for example - the Ruleset navigational property is correctly initialised to mania,
                // but the RulesetID foreign key property is not initialised and remains 0.
                // EF silently bypasses this by prioritising the Ruleset navigational property over the RulesetID foreign key one.
                //
                // Additionally, adding an entity to an EF DbSet CAUSES SIDE EFFECTS with regard to the foreign key property.
                // In the above instance, if a ScoreInfo with Ruleset = {mania} and RulesetID = 0 is attached to an EF context,
                // RulesetID WILL BE SILENTLY SET TO THE CORRECT VALUE of 3.
                //
                // For the above reasons, importing is disabled in this test.
                return Task.CompletedTask;
            }
        }
    }
}
