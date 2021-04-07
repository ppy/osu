// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.Break;
using osu.Game.Screens.Ranking;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("Player instantiated with an autoplay mod.")]
    public class TestSceneAutoplay : TestSceneAllRulesetPlayers
    {
        protected new TestPlayer Player => (TestPlayer)base.Player;

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new[] { ruleset.GetAutoplayMod() };
            return new TestPlayer(false);
        }

        protected override void AddCheckSteps()
        {
            AddUntilStep("score above zero", () => Player.ScoreProcessor.TotalScore.Value > 0);
            AddUntilStep("key counter counted keys", () => Player.HUDOverlay.KeyCounter.Children.Any(kc => kc.CountPresses > 2));
            seekToBreak(0);
            AddAssert("keys not counting", () => !Player.HUDOverlay.KeyCounter.IsCounting);
            AddAssert("overlay displays 100% accuracy", () => Player.BreakOverlay.ChildrenOfType<BreakInfo>().Single().AccuracyDisplay.Current.Value == 1);
            AddStep("rewind", () => Player.GameplayClockContainer.Seek(-80000));
            AddUntilStep("key counter reset", () => Player.HUDOverlay.KeyCounter.Children.All(kc => kc.CountPresses == 0));

            double? time = null;

            AddStep("store time", () => time = Player.GameplayClockContainer.GameplayClock.CurrentTime);

            // test seek via keyboard
            AddStep("seek with right arrow key", () => InputManager.Key(Key.Right));
            AddAssert("time seeked forward", () => Player.GameplayClockContainer.GameplayClock.CurrentTime > time + 2000);

            AddStep("store time", () => time = Player.GameplayClockContainer.GameplayClock.CurrentTime);
            AddStep("seek with left arrow key", () => InputManager.Key(Key.Left));
            AddAssert("time seeked backward", () => Player.GameplayClockContainer.GameplayClock.CurrentTime < time);

            seekToBreak(0);
            seekToBreak(1);

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));
            AddUntilStep("results displayed", () => getResultsScreen() != null);

            AddAssert("score has combo", () => getResultsScreen().Score.Combo > 100);
            AddAssert("score has no misses", () => getResultsScreen().Score.Statistics[HitResult.Miss] == 0);

            ResultsScreen getResultsScreen() => Stack.CurrentScreen as ResultsScreen;
        }

        private void seekToBreak(int breakIndex)
        {
            AddStep($"seek to break {breakIndex}", () => Player.GameplayClockContainer.Seek(destBreak().StartTime));
            AddUntilStep("wait for seek to complete", () => Player.HUDOverlay.Progress.ReferenceClock.CurrentTime >= destBreak().StartTime);

            BreakPeriod destBreak() => Beatmap.Value.Beatmap.Breaks.ElementAt(breakIndex);
        }
    }
}
