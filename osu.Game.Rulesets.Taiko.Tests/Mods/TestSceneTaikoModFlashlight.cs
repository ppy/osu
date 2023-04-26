// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public partial class TestSceneTaikoModFlashlight : TaikoModTestScene
    {
        [TestCase(1f)]
        [TestCase(0.5f)]
        [TestCase(1.25f)]
        [TestCase(1.5f)]
        public void TestSizeMultiplier(float sizeMultiplier) => CreateModTest(new ModTestData { Mod = new TaikoModFlashlight { SizeMultiplier = { Value = sizeMultiplier } }, PassCondition = () => true });

        [Test]
        public void TestComboBasedSize([Values] bool comboBasedSize) => CreateModTest(new ModTestData { Mod = new TaikoModFlashlight { ComboBasedSize = { Value = comboBasedSize } }, PassCondition = () => true });

        [Test]
        public void TestFlashlightAlwaysHasNonZeroSize()
        {
            bool failed = false;

            CreateModTest(new ModTestData
            {
                Mod = new TestTaikoModFlashlight { ComboBasedSize = { Value = true } },
                Autoplay = false,
                PassCondition = () =>
                {
                    failed |= this.ChildrenOfType<TestTaikoModFlashlight.TestTaikoFlashlight>().SingleOrDefault()?.FlashlightSize.Y == 0;
                    return !failed;
                }
            });
        }

        private partial class TestTaikoModFlashlight : TaikoModFlashlight
        {
            protected override Flashlight CreateFlashlight() => new TestTaikoFlashlight(this, Playfield);

            public partial class TestTaikoFlashlight : TaikoFlashlight
            {
                public TestTaikoFlashlight(TaikoModFlashlight modFlashlight, TaikoPlayfield taikoPlayfield)
                    : base(modFlashlight, taikoPlayfield)
                {
                }

                public new Vector2 FlashlightSize => base.FlashlightSize;
            }
        }
    }
}
