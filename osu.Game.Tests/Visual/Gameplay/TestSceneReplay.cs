// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("Player instantiated with a replay.")]
    public class TestSceneReplay : AllPlayersTestScene
    {
        protected override Player CreatePlayer(Ruleset ruleset)
        {
            var beatmap = Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo, Array.Empty<Mod>());

            return new ScoreAccessibleReplayPlayer(ruleset.GetAutoplayMod().CreateReplayScore(beatmap));
        }

        protected override void AddCheckSteps()
        {
            AddUntilStep("score above zero", () => ((ScoreAccessibleReplayPlayer)Player).ScoreProcessor.TotalScore.Value > 0);
            AddUntilStep("key counter counted keys", () => ((ScoreAccessibleReplayPlayer)Player).HUDOverlay.KeyCounter.Children.Any(kc => kc.CountPresses > 0));
            AddAssert("cannot fail", () => !((ScoreAccessibleReplayPlayer)Player).AllowFail);
        }

        private class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
            public new HUDOverlay HUDOverlay => base.HUDOverlay;
            public new bool AllowFail => base.AllowFail;

            protected override bool PauseOnFocusLost => false;

            public ScoreAccessibleReplayPlayer(Score score)
                : base(score)
            {
            }
        }
    }
}
