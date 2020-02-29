// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Storyboards;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("Player instantiated with an autoplay mod.")]
    public class TestSceneAutoplay : TestSceneAllRulesetPlayers
    {
        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Concat(new[] { ruleset.GetAutoplayMod() }).ToArray();
            return new ScoreAccessiblePlayer();
        }

        protected override void AddCheckSteps()
        {
            ScoreAccessiblePlayer scoreAccessiblePlayer = null;

            AddUntilStep("player loaded", () => (scoreAccessiblePlayer = (ScoreAccessiblePlayer)Player) != null);
            AddUntilStep("score above zero", () => scoreAccessiblePlayer.ScoreProcessor.TotalScore.Value > 0);
            AddUntilStep("key counter counted keys", () => scoreAccessiblePlayer.HUDOverlay.KeyCounter.Children.Any(kc => kc.CountPresses > 2));
            AddStep("seek to break time", () => scoreAccessiblePlayer.GameplayClockContainer.Seek(scoreAccessiblePlayer.BreakOverlay.Breaks.First().StartTime));
            AddUntilStep("wait for seek to complete", () =>
                scoreAccessiblePlayer.HUDOverlay.Progress.ReferenceClock.CurrentTime >= scoreAccessiblePlayer.BreakOverlay.Breaks.First().StartTime);
            AddAssert("test keys not counting", () => !scoreAccessiblePlayer.HUDOverlay.KeyCounter.IsCounting);
            AddStep("rewind", () => scoreAccessiblePlayer.GameplayClockContainer.Seek(-80000));
            AddUntilStep("key counter reset", () => scoreAccessiblePlayer.HUDOverlay.KeyCounter.Children.All(kc => kc.CountPresses == 0));
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
        {
            var working = base.CreateWorkingBeatmap(beatmap, storyboard);

            return working;
        }

        private class ScoreAccessiblePlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
            public new HUDOverlay HUDOverlay => base.HUDOverlay;
            public new BreakOverlay BreakOverlay => base.BreakOverlay;

            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

            public ScoreAccessiblePlayer()
                : base(false, false)
            {
            }
        }
    }
}
