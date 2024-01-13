// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestScenePlayerScoreSubmission : PlayerTestScene
    {
        protected override bool AllowFail => allowFail;

        private bool allowFail;

        private Func<RulesetInfo, IBeatmap> createCustomBeatmap;
        private Func<Ruleset> createCustomRuleset;
        private Func<Mod[]> createCustomMods;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        protected override bool HasCustomSteps => true;

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            if (createCustomMods != null)
                SelectedMods.Value = SelectedMods.Value.Concat(createCustomMods()).ToList();

            return new FakeImportingPlayer(false);
        }

        protected new FakeImportingPlayer Player => (FakeImportingPlayer)base.Player;

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
            prepareTestAPI(false);

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
            prepareTestAPI(true);

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
            prepareTestAPI(true);

            createPlayerTest(createRuleset: () => new TaikoRuleset());

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            addFakeHit();

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);
            AddAssert("ensure passing submission", () => Player.SubmittedScore?.ScoreInfo.Passed == true);
            AddAssert("submitted score has correct ruleset ID", () => Player.SubmittedScore?.ScoreInfo.Ruleset.ShortName == new TaikoRuleset().RulesetInfo.ShortName);
        }

        [Test]
        public void TestSubmissionForConvertedBeatmap()
        {
            prepareTestAPI(true);

            createPlayerTest(createRuleset: () => new ManiaRuleset(), createBeatmap: _ => createTestBeatmap(new OsuRuleset().RulesetInfo));

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            addFakeHit();

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);
            AddAssert("ensure passing submission", () => Player.SubmittedScore?.ScoreInfo.Passed == true);
            AddAssert("submitted score has correct ruleset ID", () => Player.SubmittedScore?.ScoreInfo.Ruleset.ShortName == new ManiaRuleset().RulesetInfo.ShortName);
        }

        [Test]
        public void TestNoSubmissionOnExitWithNoToken()
        {
            prepareTestAPI(false);

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
            prepareTestAPI(true);

            createPlayerTest(true);

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddUntilStep("wait for fail", () => Player.GameplayState.HasFailed);
            AddStep("exit", () => Player.Exit());

            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestSubmissionOnFail()
        {
            prepareTestAPI(true);

            createPlayerTest(true);

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            addFakeHit();

            AddUntilStep("wait for fail", () => Player.GameplayState.HasFailed);

            AddUntilStep("wait for submission", () => Player.SubmittedScore != null);
            AddAssert("ensure failing submission", () => Player.SubmittedScore.ScoreInfo.Passed == false);
        }

        [Test]
        public void TestNoSubmissionOnEmptyExit()
        {
            prepareTestAPI(true);

            createPlayerTest();

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestSubmissionOnExit()
        {
            prepareTestAPI(true);

            createPlayerTest();

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            addFakeHit();

            AddStep("exit", () => Player.Exit());

            AddUntilStep("wait for submission", () => Player.SubmittedScore != null);
            AddAssert("ensure failing submission", () => Player.SubmittedScore.ScoreInfo.Passed == false);
        }

        [Test]
        public void TestSubmissionOnExitDuringImport()
        {
            prepareTestAPI(true);

            createPlayerTest();
            AddStep("block imports", () => Player.AllowImportCompletion.Wait());

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            addFakeHit();

            AddUntilStep("wait for import to start", () => Player.ScoreImportStarted);

            AddStep("exit", () => Player.Exit());
            AddStep("allow import to proceed", () => Player.AllowImportCompletion.Release(1));
            AddUntilStep("ensure submission", () => Player.SubmittedScore != null && Player.ImportedScore != null);
        }

        [Test]
        public void TestNoSubmissionOnLocalBeatmap()
        {
            prepareTestAPI(true);

            createPlayerTest(false, r =>
            {
                var beatmap = createTestBeatmap(r);
                beatmap.BeatmapInfo.ResetOnlineInfo();
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
            prepareTestAPI(true);

            createPlayerTest(createRuleset: () => new OsuRuleset
            {
                RulesetInfo =
                {
                    Name = "custom",
                    ShortName = $"custom{rulesetId}",
                    OnlineID = rulesetId ?? -1
                }
            });

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            addFakeHit();

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        [Test]
        public void TestNoSubmissionWithModsOfDifferentRuleset()
        {
            prepareTestAPI(true);

            createPlayerTest(createRuleset: () => new OsuRuleset(), createMods: () => new Mod[] { new TaikoModHidden() });

            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddStep("exit", () => Player.Exit());
            AddAssert("ensure no submission", () => Player.SubmittedScore == null);
        }

        private void createPlayerTest(bool allowFail = false, Func<RulesetInfo, IBeatmap> createBeatmap = null, Func<Ruleset> createRuleset = null, Func<Mod[]> createMods = null)
        {
            CreateTest(() => AddStep("set up requirements", () =>
            {
                this.allowFail = allowFail;
                createCustomBeatmap = createBeatmap;
                createCustomRuleset = createRuleset;
                createCustomMods = createMods;
            }));
        }

        private void prepareTestAPI(bool validToken)
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

                        case SubmitSoloScoreRequest submissionRequest:
                            if (validToken)
                            {
                                var requestScore = submissionRequest.Score;

                                submissionRequest.TriggerSuccess(new MultiplayerScore
                                {
                                    ID = 1234,
                                    User = dummyAPI.LocalUser.Value,
                                    Rank = requestScore.Rank,
                                    TotalScore = requestScore.TotalScore,
                                    Accuracy = requestScore.Accuracy,
                                    MaxCombo = requestScore.MaxCombo,
                                    Mods = requestScore.Mods,
                                    Statistics = requestScore.Statistics,
                                    Passed = requestScore.Passed,
                                    EndedAt = DateTimeOffset.Now,
                                    Position = 1
                                });

                                return true;
                            }

                            break;
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

        protected partial class FakeImportingPlayer : TestPlayer
        {
            public bool ScoreImportStarted { get; set; }
            public SemaphoreSlim AllowImportCompletion { get; }
            public Score ImportedScore { get; private set; }

            public FakeImportingPlayer(bool allowPause = true, bool showResults = true, bool pauseOnFocusLost = false)
                : base(allowPause, showResults, pauseOnFocusLost)
            {
                AllowImportCompletion = new SemaphoreSlim(1);
            }

            protected override async Task ImportScore(Score score)
            {
                ScoreImportStarted = true;

                await AllowImportCompletion.WaitAsync().ConfigureAwait(false);

                ImportedScore = score;

                // Calling base.ImportScore is omitted as it will fail for the test method which uses a custom ruleset.
                // This can be resolved by doing something similar to what TestScenePlayerLocalScoreImport is doing,
                // but requires a bit of restructuring.
            }
        }
    }
}
