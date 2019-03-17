// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        protected override Player CreatePlayer(Ruleset ruleset) => new PausePlayer();

        protected override void AddCheckSteps(Func<Player> player)
        {
            PausePlayer pausable() => (PausePlayer)player();

            base.AddCheckSteps(player);
            //AddUntilStep(() => pausable().ScoreProcessor.TotalScore.Value > 0, "score above zero");

            AddStep("pause", () => pausable().PausableGameplayContainer.Pause());
            AddAssert("clock stopped", () => !pausable().GameplayClockContainer.GameplayClock.IsRunning);

            AddStep("resume", () => pausable().PausableGameplayContainer.Resume());
            AddUntilStep(() => pausable().GameplayClockContainer.GameplayClock.IsRunning, "clock started");

            AddStep("pause too soon", () => pausable().PausableGameplayContainer.Pause());
            AddAssert("clock not stopped", () => pausable().GameplayClockContainer.GameplayClock.IsRunning);
        }

        private class PausePlayer : Player
        {
            public new PausableGameplayContainer PausableGameplayContainer => base.PausableGameplayContainer;

            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
        }
    }
}
