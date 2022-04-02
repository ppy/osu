// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.Break;
using osu.Game.Screens.Ranking;
using osu.Game.Users.Drawables;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("Player instantiated with an autoplay mod.")]
    public class TestSceneAutoplay : TestSceneAllRulesetPlayers
    {
        protected new TestReplayPlayer Player => (TestReplayPlayer)base.Player;

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new[] { ruleset.GetAutoplayMod() };
            return new TestReplayPlayer(false);
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

            seekToBreak(0);
            seekToBreak(1);

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => getResultsScreen()?.IsLoaded == true);

            AddAssert("score has combo", () => getResultsScreen().Score.Combo > 100);
            AddAssert("score has no misses", () => getResultsScreen().Score.Statistics[HitResult.Miss] == 0);

            AddUntilStep("avatar displayed", () => getAvatar() != null);
            AddAssert("avatar not clickable", () => getAvatar().ChildrenOfType<OsuClickableContainer>().First().Action == null);

            ClickableAvatar getAvatar() => getResultsScreen()
                                           .ChildrenOfType<ClickableAvatar>().FirstOrDefault();

            ResultsScreen getResultsScreen() => Stack.CurrentScreen as ResultsScreen;
        }

        private void seekToBreak(int breakIndex)
        {
            AddStep($"seek to break {breakIndex}", () => Player.GameplayClockContainer.Seek(destBreak().StartTime));
            AddUntilStep("wait for seek to complete", () => Player.DrawableRuleset.FrameStableClock.CurrentTime >= destBreak().StartTime);

            BreakPeriod destBreak() => Beatmap.Value.Beatmap.Breaks.ElementAt(breakIndex);
        }
    }
}
