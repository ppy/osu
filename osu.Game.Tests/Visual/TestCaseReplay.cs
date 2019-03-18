// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [Description("Player instantiated with a replay.")]
    public class TestCaseReplay : AllPlayersTestCase
    {
        protected override Player CreatePlayer(Ruleset ruleset)
        {
            var beatmap = Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo);

            return new ScoreAccessibleReplayPlayer(ruleset.GetAutoplayMod().CreateReplayScore(beatmap));
        }

        protected override void AddCheckSteps()
        {
            AddUntilStep(() => ((ScoreAccessibleReplayPlayer)Player).ScoreProcessor.TotalScore.Value > 0, "score above zero");
            AddUntilStep(() => ((ScoreAccessibleReplayPlayer)Player).HUDOverlay.KeyCounter.Children.Any(kc => kc.CountPresses > 0), "key counter counted keys");
        }

        private class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
            public new HUDOverlay HUDOverlay => base.HUDOverlay;

            public ScoreAccessibleReplayPlayer(Score score)
                : base(score)
            {
            }
        }
    }
}
