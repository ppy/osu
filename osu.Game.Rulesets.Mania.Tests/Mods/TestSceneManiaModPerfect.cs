// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public class TestSceneManiaModPerfect : ModPerfectTestScene
    {
        public TestSceneManiaModPerfect()
            : base(new ManiaRuleset(), new ManiaModPerfect())
        {
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestNote(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new Note { StartTime = 1000 }), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestHoldNote(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new HoldNote { StartTime = 1000, EndTime = 3000 }), shouldMiss);
    }
}
