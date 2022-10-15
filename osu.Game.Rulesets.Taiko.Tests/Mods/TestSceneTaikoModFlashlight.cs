// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Taiko.Mods;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public class TestSceneTaikoModFlashlight : TaikoModTestScene
    {
        [TestCase(1f)]
        [TestCase(0.5f)]
        [TestCase(1.25f)]
        [TestCase(1.5f)]
        public void TestSizeMultiplier(float sizeMultiplier) => CreateModTest(new ModTestData { Mod = new TaikoModFlashlight { SizeMultiplier = { Value = sizeMultiplier } }, PassCondition = () => true });

        [Test]
        public void TestComboBasedSize([Values] bool comboBasedSize) => CreateModTest(new ModTestData { Mod = new TaikoModFlashlight { ComboBasedSize = { Value = comboBasedSize } }, PassCondition = () => true });
    }
}
