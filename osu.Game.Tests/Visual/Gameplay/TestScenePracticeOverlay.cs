// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Game.Overlays.Practice;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("Player instantiated with an autoplay mod.")]
    public class TestScenePractice : TestSceneAllRulesetPlayers
    {
        [Cached]
        private PracticePlayerLoader loader { get; set; } = new PracticePlayerLoader();

        protected new PracticePlayer Player => (PracticePlayer)base.Player;

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new[] { ruleset.GetAutoplayMod() };
            return new PracticePlayer();
        }

        protected override void AddCheckSteps()
        {
        }
    }
}
