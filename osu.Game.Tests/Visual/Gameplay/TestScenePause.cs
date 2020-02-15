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
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePause : PlayerTestScene
    {
        protected new PausePlayer Player => (PausePlayer)base.Player;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public TestScenePause()
            : base(new OsuRuleset())
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

            confirmClockRunning(false);
            confirmPauseOverlayShown(false);

            AddStep("click to resume", () =>
            {
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
            });

            confirmClockRunning(true);
        }

        [Test]
        public void TestPauseWithResumeOverlay()
        {
            AddStep("move cursor to center", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for hitobjects", () => Player.HealthProcessor.Health.Value < 1);

            pauseAndConfirm();

            resume();
            confirmClockRunning(false);
            confirmPauseOverlayShown(false);

            pauseAndConfirm();

            AddUntilStep("resume overlay is not active", () => Player.DrawableRuleset.ResumeOverlay.State.Value == Visibility.Hidden);
            confirmPaused();
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
        public void TestPauseTooSoon()
        {
            AddStep("move cursor outside", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.TopLeft - new Vector2(10)));

            pauseAndConfirm();

            resume();
            pause();

            confirmClockRunning(true);
            confirmPauseOverlayShown(false);
        }

        [Test]
        public void TestExitTooSoon()
        {
            AddStep("seek before gameplay", () => Player.GameplayClockContainer.Seek(-5000));

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
            AddUntilStep("fail overlay shown", () => Player.FailOverlayVisible);

            confirmClockRunning(false);

            pause();

            confirmClockRunning(false);
            confirmPauseOverlayShown(false);

            AddAssert("fail overlay still shown", () => Player.FailOverlayVisible);

            exitAndConfirm();
        }

        [Test]
        public void TestExitFromFailedGameplay()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddStep("exit", () => Player.Exit());

            confirmExited();
        }

        [Test]
        public void TestQuickRetryFromFailedGameplay()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddStep("quick retry", () => Player.GameplayClockContainer.OfType<HotkeyRetryOverlay>().First().Action?.Invoke());

            confirmExited();
        }

        [Test]
        public void TestQuickExitFromFailedGameplay()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddStep("quick exit", () => Player.GameplayClockContainer.OfType<HotkeyExitOverlay>().First().Action?.Invoke());

            confirmExited();
        }

        [Test]
        public void TestExitFromGameplay()
        {
            AddStep("exit", () => Player.Exit());
            confirmPaused();

            AddStep("exit", () => Player.Exit());
            confirmExited();
        }

        [Test]
        public void TestQuickExitFromGameplay()
        {
            AddStep("quick exit", () => Player.GameplayClockContainer.OfType<HotkeyExitOverlay>().First().Action?.Invoke());

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
            confirmNoTrackAdjustments();
        }

        private void confirmPaused()
        {
            confirmClockRunning(false);
            AddAssert("player not exited", () => Player.IsCurrentScreen());
            AddAssert("player not failed", () => !Player.HasFailed);
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

        private void confirmNoTrackAdjustments()
        {
            AddAssert("track has no adjustments", () => Beatmap.Value.Track.AggregateFrequency.Value == 1);
        }

        private void restart() => AddStep("restart", () => Player.Restart());
        private void pause() => AddStep("pause", () => Player.Pause());
        private void resume() => AddStep("resume", () => Player.Resume());

        private void confirmPauseOverlayShown(bool isShown) =>
            AddAssert("pause overlay " + (isShown ? "shown" : "hidden"), () => Player.PauseOverlayVisible == isShown);

        private void confirmClockRunning(bool isRunning) =>
            AddUntilStep("clock " + (isRunning ? "running" : "stopped"), () => Player.GameplayClockContainer.GameplayClock.IsRunning == isRunning);

        protected override bool AllowFail => true;

        protected override Player CreatePlayer(Ruleset ruleset) => new PausePlayer();

        protected class PausePlayer : TestPlayer
        {
            public new HealthProcessor HealthProcessor => base.HealthProcessor;

            public new HUDOverlay HUDOverlay => base.HUDOverlay;

            public bool FailOverlayVisible => FailOverlay.State.Value == Visibility.Visible;

            public bool PauseOverlayVisible => PauseOverlay.State.Value == Visibility.Visible;

            public override void OnEntering(IScreen last)
            {
                base.OnEntering(last);
                GameplayClockContainer.Stop();
            }
        }
    }
}
