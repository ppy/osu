// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Mods
{
    public partial class TestSceneModDisable : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        private TestPlayer player;

        protected override TestPlayer CreateModPlayer(Ruleset ruleset)
        {
            player = base.CreateModPlayer(ruleset);
            return player;
        }

        private void moveToMod<T>() where T : ICanBeToggledDuringReplay
        {
            AddStep($"move mouse to mod {typeof(T)}", () =>
            {
                foreach (var i in player.ChildrenOfType<ClickableModIcon>())
                {
                    if (i.Mod is T)
                    {
                        InputManager.MoveMouseTo(i);
                        break;
                    }
                }
            });
        }

        [Test]
        public void TestDisableAvailableInReplay()
        {
            CreateModTest(new ModTestData
            {
                Autoplay = true,
                Mod = new OsuModFlashlight(),
                PassCondition = () =>
                {
                    foreach (var i in player.ChildrenOfType<ClickableModIcon>())
                    {
                        if (i.Mod is OsuModFlashlight dmod)
                        {
                            return dmod.IsDisabled.Value;
                        }
                    }

                    return false;
                },
            });
            moveToMod<OsuModFlashlight>();
            AddStep("click", () => InputManager.Click(MouseButton.Left));
        }

        [Test]
        public void TestDisableAvailableNotInReplay()
        {
            CreateModTest(new ModTestData
            {
                Autoplay = false,
                Mod = new OsuModFlashlight(),
                PassCondition = () =>
                {
                    foreach (var i in player.ChildrenOfType<ClickableModIcon>())
                    {
                        if (i.Mod is OsuModFlashlight dmod)
                        {
                            return !dmod.IsDisabled.Value;
                        }
                    }

                    return false;
                },
            });
            moveToMod<OsuModFlashlight>();
            AddStep("click", () => InputManager.Click(MouseButton.Left));
        }
    }
}
