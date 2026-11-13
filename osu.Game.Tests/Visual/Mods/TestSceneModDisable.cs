// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Tests.Visual.Mods
{
    public partial class TestSceneModDisable : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        [Test]
        public void TestDisableAvailableInReplay()
        {
            CreateModTest(new ModTestData
            {
                Autoplay = true,
                Mod = new OsuModFlashlight(),
                PassCondition = () => getMod<OsuModFlashlight>().IsDisabled.Value,
            });

            clickMod<OsuModFlashlight>();
        }

        [Test]
        public void TestDisableAvailableNotInReplay()
        {
            CreateModTest(new ModTestData
            {
                Autoplay = false,
                Mod = new OsuModFlashlight(),
                PassCondition = () => !getMod<OsuModFlashlight>().IsDisabled.Value,
            });

            clickMod<OsuModFlashlight>();
        }

        private void clickMod<T>()
            where T : IAdjustableWhenReplay
        {
            AddStep("click mod", () => Player.ChildrenOfType<ModIcon>().Single(icon => icon.Mod is T).TriggerClick());
        }

        private T getMod<T>()
            where T : IAdjustableWhenReplay
            => (T)Player.ChildrenOfType<ModIcon>().Single(icon => icon.Mod is T).Mod;
    }
}
