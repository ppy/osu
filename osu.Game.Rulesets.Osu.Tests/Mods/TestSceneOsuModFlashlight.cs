// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModFlashlight : OsuModTestScene
    {
        [TestCase(600)]
        [TestCase(120)]
        [TestCase(1200)]
        public void TestFollowDelay(double followDelay) => CreateModTest(new ModTestData { Mod = new OsuModFlashlight { FollowDelay = { Value = followDelay } }, PassCondition = () => true });

        [TestCase(1f)]
        [TestCase(0.5f)]
        [TestCase(1.5f)]
        [TestCase(2f)]
        public void TestSizeMultiplier(float sizeMultiplier) => CreateModTest(new ModTestData { Mod = new OsuModFlashlight { SizeMultiplier = { Value = sizeMultiplier } }, PassCondition = () => true });

        [Test]
        public void TestComboBasedSize([Values] bool comboBasedSize) => CreateModTest(new ModTestData { Mod = new OsuModFlashlight { ComboBasedSize = { Value = comboBasedSize } }, PassCondition = () => true });

        [Test]
        public void TestPlayfieldBasedSize()
        {
            ModFlashlight mod = new OsuModFlashlight();
            CreateModTest(new ModTestData
            {
                Mod = mod,
                PassCondition = () =>
                {
                    var flashlightOverlay = Player.DrawableRuleset.Overlays
                                                  .OfType<ModFlashlight<OsuHitObject>.Flashlight>()
                                                  .First();

                    return Precision.AlmostEquals(mod.DefaultFlashlightSize * .5f, flashlightOverlay.GetSize());
                }
            });

            AddStep("adjust playfield scale", () =>
                Player.DrawableRuleset.Playfield.Scale = new Vector2(.5f));
        }
    }
}
