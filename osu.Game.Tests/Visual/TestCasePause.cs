// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public class TestCasePause : TestCasePlayer
    {
        public TestCasePause()
            : base(new OsuRuleset())
        {
        }

        protected override bool AllowFail => true;

        protected override Player CreatePlayer(Ruleset ruleset) => new PausePlayer();

        protected override void AddCheckSteps(Func<Player> player)
        {
            PausePlayer pausable() => (PausePlayer)player();

            base.AddCheckSteps(player);

            AddStep("pause", () => pausable().Pause());
            AddAssert("clock stopped", () => !pausable().GameplayClockContainer.GameplayClock.IsRunning);
            AddAssert("pause overlay shown", () => pausable().PauseOverlayVisible);

            AddStep("resume", () => pausable().Resume());
            AddAssert("pause overlay hidden", () => !pausable().PauseOverlayVisible);

            AddStep("pause too soon", () => pausable().Pause());
            AddAssert("clock not stopped", () => pausable().GameplayClockContainer.GameplayClock.IsRunning);
            AddAssert("pause overlay hidden", () => !pausable().PauseOverlayVisible);

            AddUntilStep(() => pausable().HasFailed, "wait for fail");

            AddAssert("fail overlay shown", () => pausable().FailOverlayVisible);

            AddStep("try to pause", () => pausable().Pause());

            AddAssert("pause overlay hidden", () => !pausable().PauseOverlayVisible);
            AddAssert("fail overlay still shown", () => pausable().FailOverlayVisible);

            AddStep("restart", () => pausable().Restart());

            AddUntilStep(() =>
            {
                pausable().Pause();
                return pausable().PauseOverlayVisible;
            }, "keep trying to pause");

            AddStep("exit", () => pausable().Exit());
            AddUntilStep(() => !pausable().IsCurrentScreen(), "player exited");
        }

        private class PausePlayer : Player
        {
            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public bool FailOverlayVisible => FailOverlay.State == Visibility.Visible;

            public bool PauseOverlayVisible => PauseOverlay.State == Visibility.Visible;
        }
    }
}
