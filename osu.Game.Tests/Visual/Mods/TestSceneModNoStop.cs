// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Mods
{
    public partial class TestSceneModNoStop : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override TestPlayer CreateModPlayer(Ruleset ruleset)
        {
            var player = base.CreateModPlayer(ruleset);
            return player;
        }

        [Test]
        public void TestScenePauseToQuit() => CreateModTest(new ModTestData
        {
            Mod = new ModNoStop(),
            Autoplay = false,
            PassCondition = () =>
            {
                InputManager.PressKey(Key.Escape);
                return Player.GameplayState.HasQuit;
            }
        });
    }
}
