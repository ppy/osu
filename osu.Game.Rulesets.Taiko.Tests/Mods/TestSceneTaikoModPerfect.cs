// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public class TestSceneTaikoModPerfect : ModPerfectTestScene
    {
        public TestSceneTaikoModPerfect()
            : base(new TestTaikoRuleset(), new TaikoModPerfect())
        {
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestHit(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new CentreHit { StartTime = 1000 }), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestDrumRoll(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new DrumRoll { StartTime = 1000, EndTime = 3000 }), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestSwell(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new Swell { StartTime = 1000, EndTime = 3000 }), shouldMiss);

        private class TestTaikoRuleset : TaikoRuleset
        {
            public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new TestTaikoHealthProcessor();

            private class TestTaikoHealthProcessor : TaikoHealthProcessor
            {
                protected override void Reset(bool storeResults)
                {
                    base.Reset(storeResults);

                    Health.Value = 1; // Don't care about the health condition (only the mod condition)
                }
            }
        }
    }
}
