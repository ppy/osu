// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public class TestCasePause : PlayerTestCase
    {
        protected new PausePlayer Player => (PausePlayer)base.Player;

        public TestCasePause()
            : base(new OsuRuleset())
        {
        }

        [Test]
        public void TestPauseResume()
        {
            pauseAndConfirm();
            resumeAndConfirm();
        }

        [Test]
        public void TestPauseTooSoon()
        {
            pauseAndConfirm();
            resumeAndConfirm();

            pause();

            confirmClockRunning(true);
            confirmPauseOverlayShown(false);
        }

        [Test]
        public void TestExitTooSoon()
        {
            pauseAndConfirm();

            resume();

            AddStep("exit too soon", () => Player.Exit());

            confirmClockRunning(true);
            confirmPauseOverlayShown(false);

            AddAssert("not exited", () => Player.IsCurrentScreen());
        }

        [Test]
        public void TestPauseAfterFail()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddAssert("fail overlay shown", () => Player.FailOverlayVisible);

            confirmClockRunning(false);

            pause();

            confirmClockRunning(false);
            confirmPauseOverlayShown(false);

            AddAssert("fail overlay still shown", () => Player.FailOverlayVisible);

            exitAndConfirm();
        }

        [Test]
        public void TestExitFromGameplay()
        {
            AddStep("exit", () => Player.Exit());

            confirmPaused();

            exitAndConfirm();
        }

        [Test]
        public void TestExitFromPause()
        {
            pauseAndConfirm();
            exitAndConfirm();
        }

        private void pauseAndConfirm()
        {
            pause();
            confirmPaused();
        }

        private void resumeAndConfirm()
        {
            resume();
            confirmResumed();
        }

        private void exitAndConfirm()
        {
            AddUntilStep("player not exited", () => Player.IsCurrentScreen());
            AddStep("exit", () => Player.Exit());
            confirmExited();
        }

        private void confirmPaused()
        {
            confirmClockRunning(false);
            AddAssert("pause overlay shown", () => Player.PauseOverlayVisible);
        }

        private void confirmResumed()
        {
            confirmClockRunning(true);
            confirmPauseOverlayShown(false);
        }

        private void confirmExited()
        {
            AddUntilStep("player exited", () => !Player.IsCurrentScreen());
        }

        private void pause() => AddStep("pause", () => Player.Pause());
        private void resume() => AddStep("resume", () => Player.Resume());

        private void confirmPauseOverlayShown(bool isShown) =>
            AddAssert("pause overlay " + (isShown ? "shown" : "hidden"), () => Player.PauseOverlayVisible == isShown);

        private void confirmClockRunning(bool isRunning) =>
            AddAssert("clock " + (isRunning ? "running" : "stopped"), () => Player.GameplayClockContainer.GameplayClock.IsRunning == isRunning);

        protected override bool AllowFail => true;

        protected override Player CreatePlayer(Ruleset ruleset) => new PausePlayer();

        protected class PausePlayer : Player
        {
            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public bool FailOverlayVisible => FailOverlay.State == Visibility.Visible;

            public bool PauseOverlayVisible => PauseOverlay.State == Visibility.Visible;
        }
    }
}
