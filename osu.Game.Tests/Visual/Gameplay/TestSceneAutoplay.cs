// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("Player instantiated with an autoplay mod.")]
    public class TestSceneAutoplay : TestSceneAllRulesetPlayers
    {
        protected new TestPlayer Player => (TestPlayer)base.Player;

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Concat(new[] { ruleset.GetAutoplayMod() }).ToArray();
            return new TestPlayer(false, false);
        }

        protected override void AddCheckSteps()
        {
            AddUntilStep("score above zero", () => Player.ScoreProcessor.TotalScore.Value > 0);
            AddUntilStep("key counter counted keys", () => Player.HUDOverlay.KeyCounter.Children.Any(kc => kc.CountPresses > 2));
            AddStep("seek to break time", () => Player.GameplayClockContainer.Seek(Player.BreakOverlay.Breaks.First().StartTime));
            AddUntilStep("wait for seek to complete", () =>
                Player.HUDOverlay.Progress.ReferenceClock.CurrentTime >= Player.BreakOverlay.Breaks.First().StartTime);
            AddAssert("test keys not counting", () => !Player.HUDOverlay.KeyCounter.IsCounting);
            AddStep("rewind", () => Player.GameplayClockContainer.Seek(-80000));
            AddUntilStep("key counter reset", () => Player.HUDOverlay.KeyCounter.Children.All(kc => kc.CountPresses == 0));
        }
    }
}
