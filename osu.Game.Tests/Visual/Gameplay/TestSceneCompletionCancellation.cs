// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Storyboards;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneCompletionCancellation : OsuPlayerTestScene
    {
        [Resolved]
        private AudioManager audio { get; set; }

        private int resultsDisplayWaitCount =>
            (int)((Screens.Play.Player.RESULTS_DISPLAY_DELAY / TimePerAction) * 2);

        protected override bool AllowFail => false;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            // Ensure track has actually running before attempting to seek
            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
        }

        [Test]
        public void TestCancelCompletionOnRewind()
        {
            complete();
            cancel();

            checkNoRanking();
        }

        [Test]
        public void TestReCompleteAfterCancellation()
        {
            complete();
            cancel();
            complete();

            AddUntilStep("attempted to push ranking", () => ((FakeRankingPushPlayer)Player).ResultsCreated);
        }

        /// <summary>
        /// Tests whether can still pause after cancelling completion by reverting <see cref="IScreen.ValidForResume"/> back to true.
        /// </summary>
        [Test]
        public void TestCanPauseAfterCancellation()
        {
            complete();
            cancel();

            AddStep("pause", () => Player.Pause());
            AddAssert("paused successfully", () => Player.GameplayClockContainer.IsPaused.Value);

            checkNoRanking();
        }

        private void complete()
        {
            AddStep("seek to completion", () => Beatmap.Value.Track.Seek(5000));
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);
        }

        private void cancel()
        {
            AddStep("rewind to cancel", () => Beatmap.Value.Track.Seek(4000));
            AddUntilStep("completion cleared by processor", () => !Player.ScoreProcessor.HasCompleted.Value);
        }

        private void checkNoRanking()
        {
            // wait to ensure there was no attempt of pushing the results screen.
            AddWaitStep("wait", resultsDisplayWaitCount);
            AddAssert("no attempt to push ranking", () => !((FakeRankingPushPlayer)Player).ResultsCreated);
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audio);

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap();

            for (int i = 1; i <= 19; i++)
            {
                beatmap.HitObjects.Add(new HitCircle
                {
                    Position = new Vector2(256, 192),
                    StartTime = i * 250,
                });
            }

            return beatmap;
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new FakeRankingPushPlayer();

        public class FakeRankingPushPlayer : TestPlayer
        {
            public bool ResultsCreated { get; private set; }

            public FakeRankingPushPlayer()
                : base(true, true)
            {
            }

            protected override ResultsScreen CreateResults(ScoreInfo score)
            {
                var results = base.CreateResults(score);
                ResultsCreated = true;
                return results;
            }
        }
    }
}
