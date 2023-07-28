// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;
using System.Linq;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.Break;
using osu.Game.Screens.Ranking;
using osu.Game.Users.Drawables;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("Player instantiated with an autoplay mod.")]
    public partial class TestSceneAutoplay : TestSceneAllRulesetPlayers
    {
        protected new TestReplayPlayer Player => (TestReplayPlayer)base.Player;

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new[] { ruleset.GetAutoplayMod() };
            return new TestReplayPlayer(false);
        }

        protected override void AddCheckSteps()
        {
            // we only want this beatmap for time reference.
            var referenceBeatmap = CreateBeatmap(new OsuRuleset().RulesetInfo);

            AddUntilStep("score above zero", () => Player.ScoreProcessor.TotalScore.Value > 0);
            AddUntilStep("key counter counted keys", () => Player.HUDOverlay.InputCountController.Triggers.Any(kc => kc.ActivationCount.Value > 2));

            seekTo(referenceBeatmap.Breaks[0].StartTime);
            AddAssert("keys not counting", () => !Player.HUDOverlay.InputCountController.IsCounting.Value);
            AddAssert("overlay displays 100% accuracy", () => Player.BreakOverlay.ChildrenOfType<BreakInfo>().Single().AccuracyDisplay.Current.Value == 1);

            AddStep("rewind", () => Player.GameplayClockContainer.Seek(-80000));
            AddUntilStep("key counter reset", () => Player.HUDOverlay.InputCountController.Triggers.All(kc => kc.ActivationCount.Value == 0));

            seekTo(referenceBeatmap.HitObjects[^1].GetEndTime());
            AddUntilStep("results displayed", () => getResultsScreen()?.IsLoaded == true);

            AddAssert("score has combo", () => getResultsScreen().Score.Combo > 100);
            AddAssert("score has no misses", () => getResultsScreen().Score.Statistics[HitResult.Miss] == 0);

            AddUntilStep("avatar displayed", () => getAvatar() != null);
            AddAssert("avatar not clickable", () => getAvatar().ChildrenOfType<OsuClickableContainer>().First().Action == null);

            ClickableAvatar getAvatar() => getResultsScreen()
                                           .ChildrenOfType<ClickableAvatar>().FirstOrDefault();

            ResultsScreen getResultsScreen() => Stack.CurrentScreen as ResultsScreen;
        }

        private void seekTo(double time)
        {
            AddStep($"seek to {time}", () => Player.GameplayClockContainer.Seek(time));

            // Prevent test timeouts by seeking in 10 second increments.
            for (double t = 0; t < time; t += 10000)
            {
                double expectedTime = t;
                AddUntilStep($"current time >= {t}", () => Player.DrawableRuleset.FrameStableClock.CurrentTime >= expectedTime);
            }

            AddUntilStep("wait for seek to complete", () => Player.DrawableRuleset.FrameStableClock.CurrentTime >= time);
        }
    }
}
