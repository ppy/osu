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
        private ClockBackedTestWorkingBeatmap.TrackVirtualManual track;

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Concat(new[] { ruleset.GetAutoplayMod() }).ToArray();
            return new ScoreAccessiblePlayer();
        }

        protected override void AddCheckSteps()
        {
            AddUntilStep("score above zero", () => ((ScoreAccessiblePlayer)Player).ScoreProcessor.TotalScore.Value > 0);
            AddUntilStep("key counter counted keys", () => ((ScoreAccessiblePlayer)Player).HUDOverlay.KeyCounter.Children.Any(kc => kc.CountPresses > 2));
            AddStep("rewind", () => track.Seek(-10000));
            AddUntilStep("key counter reset", () => ((ScoreAccessiblePlayer)Player).HUDOverlay.KeyCounter.Children.All(kc => kc.CountPresses == 0));
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
        {
            var working = base.CreateWorkingBeatmap(beatmap, storyboard);

            track = (ClockBackedTestWorkingBeatmap.TrackVirtualManual)working.Track;

            return working;
        }

        private class ScoreAccessiblePlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
            public new HUDOverlay HUDOverlay => base.HUDOverlay;

            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

            public ScoreAccessiblePlayer()
                : base(false, false)
            {
            }
        }
    }
}
