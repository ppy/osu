// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneModTouchDevice : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestOverlayVisibleWithMod() => CreateModTest(new ModTestData
        {
            Mod = new ModTouchDevice(),
            Autoplay = false,
            PassCondition = () => getSkinnableOverlay()?.IsPresent == true
        });

        [Test]
        public void TestOverlayNotVisibleWithoutMod() => CreateModTest(new ModTestData
        {
            Autoplay = false,
            PassCondition = () => getSkinnableOverlay()?.IsPresent == false
        });

        [Test]
        public void TestPressReceptors()
        {
            CreateModTest(new ModTestData
            {
                Mod = new ModTouchDevice(),
                Autoplay = false,
                PassCondition = () => true
            });

            for (int i = 0; i < 4; i++)
            {
                int index = i;

                AddStep($"touch receptor {index}", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, getReceptor(index).ScreenSpaceDrawQuad.Centre)));

                AddAssert("action sent",
                    () => this.ChildrenOfType<ManiaInputManager>().SelectMany(m => m.KeyBindingContainer.PressedActions),
                    () => Does.Contain(getReceptor(index).Action.Value));

                AddStep($"release receptor {index}", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, getReceptor(index).ScreenSpaceDrawQuad.Centre)));
            }
        }

        private Drawable? getSkinnableOverlay() => this.ChildrenOfType<SkinnableDrawable>()
                                                       .SingleOrDefault(d => d.Lookup.Equals(new ManiaSkinComponentLookup(ManiaSkinComponents.TouchOverlay)));

        private ManiaTouchInputOverlay.InputReceptor getReceptor(int index) => this.ChildrenOfType<ManiaTouchInputOverlay.InputReceptor>().ElementAt(index);
    }
}
