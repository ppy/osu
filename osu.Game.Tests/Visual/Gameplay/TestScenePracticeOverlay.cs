// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Practice;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestScenePracticeOverlay : TestSceneAllRulesetPlayers
    {
        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new[] { ruleset.GetAutoplayMod() };
            return new PracticePlayer(new PracticePlayerLoader());
        }

        protected override void AddCheckSteps()
        {
        }
    }
}
