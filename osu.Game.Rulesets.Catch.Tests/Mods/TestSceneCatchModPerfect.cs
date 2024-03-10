// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests.Mods
{
    public partial class TestSceneCatchModPerfect : ModFailConditionTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        public TestSceneCatchModPerfect()
            : base(new CatchModPerfect())
        {
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestBananaShower(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new BananaShower { StartTime = 1000, EndTime = 3000 }, false), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestFruit(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new Fruit { StartTime = 1000 }), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestJuiceStream(bool shouldMiss)
        {
            var stream = new JuiceStream
            {
                StartTime = 1000,
                Path = new SliderPath(PathType.LINEAR, new[]
                {
                    Vector2.Zero,
                    new Vector2(100, 0),
                })
            };

            CreateHitObjectTest(new HitObjectTestData(stream), shouldMiss);
        }

        // We only care about testing misses, hits are tested via JuiceStream
        [TestCase(true)]
        public void TestDroplet(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new Droplet { StartTime = 1000 }), shouldMiss);

        // We only care about testing misses, hits are tested via JuiceStream
        [TestCase(true)]
        public void TestTinyDroplet(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new TinyDroplet { StartTime = 1000 }), shouldMiss);
    }
}
