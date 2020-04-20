// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Storyboards;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneCompletionCancellation : PlayerTestScene
    {
        private Track track;

        [Resolved]
        private AudioManager audio { get; set; }

        private int resultsDisplayWaitCount =>
            (int)((Screens.Play.Player.RESULTS_DISPLAY_DELAY / TimePerAction) * 2);

        protected override bool AllowFail => false;

        public TestSceneCompletionCancellation()
            : base(new OsuRuleset())
        {
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            // Ensure track has actually running before attempting to seek
            AddUntilStep("wait for track to start running", () => track.IsRunning);
        }

        [Test]
        public void TestCancelCompletionOnRewind()
        {
            cancelCompletionSteps();

            AddAssert("no attempt to push ranking", () => !((FakeRankingPushPlayer)Player).GotoRankingInvoked);
        }

        [Test]
        public void TestReCompleteAfterCancellation()
        {
            cancelCompletionSteps();

            // Attempt completing again.
            AddStep("seek to completion again", () => track.Seek(5000));
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);

            AddWaitStep("wait", 5);

            AddAssert("attempted to push ranking", () => ((FakeRankingPushPlayer)Player).GotoRankingInvoked);
        }

        /// <summary>
        /// Tests whether can still pause after cancelling completion
        /// by reverting <see cref="IScreen.ValidForResume"/> back to true.
        /// </summary>
        [Test]
        public void TestCanPauseAfterCancellation()
        {
            cancelCompletionSteps();

            AddStep("pause", () => Player.Pause());
            AddAssert("paused successfully", () => Player.GameplayClockContainer.IsPaused.Value);
        }

        private void cancelCompletionSteps()
        {
            AddStep("seek to completion", () => track.Seek(5000));
            AddUntilStep("completion set by processor", () => Player.ScoreProcessor.HasCompleted.Value);

            AddStep("rewind to cancel", () => track.Seek(4000));
            AddUntilStep("completion cleared by processor", () => !Player.ScoreProcessor.HasCompleted.Value);

            // wait to ensure there was no attempt of pushing the results screen.
            AddWaitStep("wait", resultsDisplayWaitCount);
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
        {
            var working = new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audio);
            track = working.Track;
            return working;
        }

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
            public bool GotoRankingInvoked;

            public FakeRankingPushPlayer()
                : base(true, true)
            {
            }

            protected override void GotoRanking()
            {
                GotoRankingInvoked = true;
            }
        }
    }
}
