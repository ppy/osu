// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Overlays.Practice;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePracticeOverlay : TestSceneAllRulesetPlayers
    {
        [Cached]
        private PracticePlayerLoader loader { get; set; } = new PracticePlayerLoader();

        private PracticePlayer testPracticePlayer = null!;

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo);

            return testPracticePlayer = new PracticePlayer();
        }

        protected override void AddCheckSteps()
        {
            AddStep("Show practice overlay", () => testPracticePlayer.PracticeOverlay.Show());
        }
    }
}
