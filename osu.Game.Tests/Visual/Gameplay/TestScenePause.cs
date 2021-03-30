// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePause : OsuPlayerTestScene
    {
        protected new PausePlayer Player => (PausePlayer)base.Player;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public TestScenePause()
        {
            base.Content.Add(content = new MenuCursorContainer { RelativeSizeAxes = Axes.Both });
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("resume player", () => Player.GameplayClockContainer.Start());
            confirmClockRunning(true);
        }

        [Test]
        public void TestPauseResume()
        {
            AddStep("move cursor outside", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.TopLeft - new Vector2(10)));
            pauseAndConfirm();
            resumeAndConfirm();
        }

        [Test]
        public void TestResumeWithResumeOverlay()
        {
            AddStep("move cursor to center", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for hitobjects", () => Player.HealthProcessor.Health.Value < 1);

            pauseAndConfirm();
            resume();

            confirmPausedWithNoOverlay();
            AddStep("click to resume", () => InputManager.Click(MouseButton.Left));

            confirmClockRunning(true);
        }

        [Test]
        public void TestPauseWithResumeOverlay()
        {
            AddStep("move cursor to center", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for hitobjects", () => Player.HealthProcessor.Health.Value < 1);

            pauseAndConfirm();
            resume();

            confirmPausedWithNoOverlay();
            pauseAndConfirm();

            AddUntilStep("resume overlay is not active", () => Player.DrawableRuleset.ResumeOverlay.State.Value == Visibility.Hidden);
            confirmPaused();
            confirmNotExited();
        }

        [Test]
        public void TestResumeWithResumeOverlaySkipped()
        {
            AddStep("move cursor to button", () =>
                InputManager.MoveMouseTo(Player.HUDOverlay.HoldToQuit.Children.OfType<HoldToConfirmContainer>().First().ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for hitobjects", () => Player.HealthProcessor.Health.Value < 1);

            pauseAndConfirm();
            resumeAndConfirm();
        }

        [Test]
        public void TestUserPauseWhenPauseNotAllowed()
        {
            AddStep("disable pause support", () => Player.Configuration.AllowPause = false);

            pauseFromUserExitKey();
            confirmExited();
        }

        [Test]
        public void TestUserPauseDuringCooldownTooSoon()
        {
            AddStep("move cursor outside", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.TopLeft - new Vector2(10)));

            pauseAndConfirm();

            resume();
            pauseFromUserExitKey();

            confirmResumed();
            confirmNotExited();
        }

        [Test]
        public void TestQuickExitDuringCooldownTooSoon()
        {
            AddStep("move cursor outside", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.TopLeft - new Vector2(10)));

            pauseAndConfirm();

            resume();
            AddStep("pause via exit key", () => Player.ExitViaQuickExit());

            confirmResumed();
            AddAssert("exited", () => !Player.IsCurrentScreen());
        }

        [Test]
        public void TestExitSoonAfterResumeSucceeds()
        {
            AddStep("seek before gameplay", () => Player.GameplayClockContainer.Seek(-5000));

            pauseAndConfirm();
            resume();

            AddStep("exit quick", () => Player.Exit());

            confirmResumed();
            AddAssert("exited", () => !Player.IsCurrentScreen());
        }

        [Test]
        public void TestPauseAfterFail()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddUntilStep("fail overlay shown", () => Player.FailOverlayVisible);

            confirmClockRunning(false);

            AddStep("pause via forced pause", () => Player.Pause());

            confirmPausedWithNoOverlay();
            AddAssert("fail overlay still shown", () => Player.FailOverlayVisible);

            exitAndConfirm();
        }

        [Test]
        public void TestExitFromFailedGameplayAfterFailAnimation()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddUntilStep("wait for fail overlay shown", () => Player.FailOverlayVisible);

            confirmClockRunning(false);

            AddStep("exit via user pause", () => Player.ExitViaPause());
            confirmExited();
        }

        [Test]
        public void TestExitFromFailedGameplayDuringFailAnimation()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);

            // will finish the fail animation and show the fail/pause screen.
            AddStep("attempt exit via pause key", () => Player.ExitViaPause());
            AddAssert("fail overlay shown", () => Player.FailOverlayVisible);

            // will actually exit.
            AddStep("exit via pause key", () => Player.ExitViaPause());
            confirmExited();
        }

        [Test]
        public void TestQuickRetryFromFailedGameplay()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddStep("quick retry", () => Player.GameplayClockContainer.ChildrenOfType<HotkeyRetryOverlay>().First().Action?.Invoke());

            confirmExited();
        }

        [Test]
        public void TestQuickExitFromFailedGameplay()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddStep("quick exit", () => Player.GameplayClockContainer.ChildrenOfType<HotkeyExitOverlay>().First().Action?.Invoke());

            confirmExited();
        }

        [Test]
        public void TestExitFromGameplay()
        {
            // an externally triggered exit should immediately exit, skipping all pause logic.
            AddStep("exit", () => Player.Exit());
            confirmExited();
        }

        [Test]
        public void TestQuickExitFromGameplay()
        {
            AddStep("quick exit", () => Player.GameplayClockContainer.ChildrenOfType<HotkeyExitOverlay>().First().Action?.Invoke());

            confirmExited();
        }

        [Test]
        public void TestExitViaHoldToExit()
        {
            AddStep("exit", () =>
            {
                InputManager.MoveMouseTo(Player.HUDOverlay.HoldToQuit.First(c => c is HoldToConfirmContainer));
                InputManager.PressButton(MouseButton.Left);
            });

            confirmPaused();

            AddStep("release", () => InputManager.ReleaseButton(MouseButton.Left));

            exitAndConfirm();
        }

        [Test]
        public void TestExitFromPause()
        {
            pauseAndConfirm();
            exitAndConfirm();
        }

        [Test]
        public void TestRestartAfterResume()
        {
            AddStep("seek before gameplay", () => Player.GameplayClockContainer.Seek(-5000));

            pauseAndConfirm();
            resumeAndConfirm();
            restart();
            confirmExited();
        }

        [Test]
        public void TestPauseSoundLoop()
        {
            AddStep("seek before gameplay", () => Player.GameplayClockContainer.Seek(-5000));

            SkinnableSound getLoop() => Player.ChildrenOfType<PauseOverlay>().FirstOrDefault()?.ChildrenOfType<SkinnableSound>().FirstOrDefault();

            pauseAndConfirm();
            AddAssert("loop is playing", () => getLoop().IsPlaying);

            resumeAndConfirm();
            AddUntilStep("loop is stopped", () => !getLoop().IsPlaying);

            AddUntilStep("pause again", () =>
            {
                Player.Pause();
                return !Player.GameplayClockContainer.GameplayClock.IsRunning;
            });

            AddAssert("loop is playing", () => getLoop().IsPlaying);

            resumeAndConfirm();
            AddUntilStep("loop is stopped", () => !getLoop().IsPlaying);
        }

        private void pauseAndConfirm()
        {
            pauseFromUserExitKey();
            confirmPaused();
        }

        private void resumeAndConfirm()
        {
            resume();
            confirmResumed();
        }

        private void exitAndConfirm()
        {
            confirmNotExited();
            AddStep("exit", () => Player.Exit());
            confirmExited();
            confirmNoTrackAdjustments();
        }

        private void confirmPaused()
        {
            confirmClockRunning(false);
            confirmNotExited();
            AddAssert("player not failed", () => !Player.HasFailed);
            AddAssert("pause overlay shown", () => Player.PauseOverlayVisible);
        }

        private void confirmResumed()
        {
            confirmClockRunning(true);
            confirmPauseOverlayShown(false);
        }

        private void confirmPausedWithNoOverlay()
        {
            confirmClockRunning(false);
            confirmPauseOverlayShown(false);
        }

        private void confirmExited() => AddUntilStep("player exited", () => !Player.IsCurrentScreen());
        private void confirmNotExited() => AddAssert("player not exited", () => Player.IsCurrentScreen());

        private void confirmNoTrackAdjustments()
        {
            AddAssert("track has no adjustments", () => Beatmap.Value.Track.AggregateFrequency.Value == 1);
        }

        private void restart() => AddStep("restart", () => Player.Restart());
        private void pauseFromUserExitKey() => AddStep("user pause", () => Player.ExitViaPause());
        private void resume() => AddStep("resume", () => Player.Resume());

        private void confirmPauseOverlayShown(bool isShown) =>
            AddAssert("pause overlay " + (isShown ? "shown" : "hidden"), () => Player.PauseOverlayVisible == isShown);

        private void confirmClockRunning(bool isRunning) =>
            AddUntilStep("clock " + (isRunning ? "running" : "stopped"), () => Player.GameplayClockContainer.GameplayClock.IsRunning == isRunning);

        protected override bool AllowFail => true;

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new PausePlayer();

        protected class PausePlayer : TestPlayer
        {
            public bool FailOverlayVisible => FailOverlay.State.Value == Visibility.Visible;

            public bool PauseOverlayVisible => PauseOverlay.State.Value == Visibility.Visible;

            public void ExitViaPause() => PerformExit(true);

            public void ExitViaQuickExit() => PerformExit(false);

            public override void OnEntering(IScreen last)
            {
                base.OnEntering(last);
                GameplayClockContainer.Stop();
            }
        }
    }
}
