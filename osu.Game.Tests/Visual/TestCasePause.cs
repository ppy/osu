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
            AddStep("pause", () => Player.Pause());
            AddAssert("clock stopped", () => !Player.GameplayClockContainer.GameplayClock.IsRunning);
            AddAssert("pause overlay shown", () => Player.PauseOverlayVisible);

            AddStep("resume", () => Player.Resume());
            AddAssert("pause overlay hidden", () => !Player.PauseOverlayVisible);
        }

        [Test]
        public void TestPauseTooSoon()
        {
            AddStep("pause", () => Player.Pause());
            AddAssert("clock stopped", () => !Player.GameplayClockContainer.GameplayClock.IsRunning);
            AddStep("resume", () => Player.Resume());
            AddAssert("clock started", () => Player.GameplayClockContainer.GameplayClock.IsRunning);
            AddStep("pause too soon", () => Player.Pause());
            AddAssert("clock not stopped", () => Player.GameplayClockContainer.GameplayClock.IsRunning);
            AddAssert("pause overlay hidden", () => !Player.PauseOverlayVisible);
        }

        [Test]
        public void TestPauseAfterFail()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);

            AddAssert("fail overlay shown", () => Player.FailOverlayVisible);

            AddStep("try to pause", () => Player.Pause());

            AddAssert("pause overlay hidden", () => !Player.PauseOverlayVisible);
            AddAssert("fail overlay still shown", () => Player.FailOverlayVisible);
        }

        [Test]
        public void TestExitFromPause()
        {
            AddUntilStep("keep trying to pause", () =>
            {
                Player.Pause();
                return Player.PauseOverlayVisible;
            });

            AddStep("exit", () => Player.Exit());
            AddUntilStep("player exited", () => !Player.IsCurrentScreen());
        }

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
