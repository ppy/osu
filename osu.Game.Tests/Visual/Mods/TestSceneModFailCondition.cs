// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Mods
{
    public partial class TestSceneModFailCondition : ModTestScene
    {
        private bool restartRequested;

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override TestPlayer CreateModPlayer(Ruleset ruleset)
        {
            var player = base.CreateModPlayer(ruleset);
            player.RestartRequested = _ => restartRequested = true;
            return player;
        }

        protected override bool AllowFail => true;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("reset flag", () => restartRequested = false);
        }

        [Test]
        public void TestRestartOnFailDisabled() => CreateModTest(new ModTestData
        {
            Autoplay = false,
            Mod = new OsuModSuddenDeath(),
            PassCondition = () => !restartRequested && Player.ChildrenOfType<FailOverlay>().Single().State.Value == Visibility.Visible
        });

        [Test]
        public void TestRestartOnFailEnabled() => CreateModTest(new ModTestData
        {
            Autoplay = false,
            Mod = new OsuModSuddenDeath
            {
                Restart = { Value = true }
            },
            PassCondition = () => restartRequested && Player.ChildrenOfType<FailOverlay>().Single().State.Value == Visibility.Hidden
        });
    }
}
