// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestSceneTaikoModPerfect : ModPerfectTestScene
    {
        public TestSceneTaikoModPerfect()
            : base(new TaikoRuleset(), new TaikoModPerfect())
        {
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestHit(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestCase(new CentreHit { StartTime = 1000 }), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestDrumRoll(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestCase(new DrumRoll { StartTime = 1000, EndTime = 3000 }), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestSwell(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestCase(new Swell { StartTime = 1000, EndTime = 3000 }), shouldMiss);
    }
}
