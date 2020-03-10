// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModPerfect : ModPerfectTestScene
    {
        public TestSceneOsuModPerfect()
            : base(new OsuRuleset(), new OsuModPerfect())
        {
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestHitCircle(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new HitCircle { StartTime = 1000 }), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestSlider(bool shouldMiss)
        {
            var slider = new Slider
            {
                StartTime = 1000,
                Path = new SliderPath(PathType.Linear, new[] { Vector2.Zero, new Vector2(100, 0), })
            };

            CreateHitObjectTest(new HitObjectTestData(slider), shouldMiss);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestSpinner(bool shouldMiss)
        {
            var spinner = new Spinner
            {
                StartTime = 1000,
                EndTime = 3000,
                Position = new Vector2(256, 192)
            };

            CreateHitObjectTest(new HitObjectTestData(spinner), shouldMiss);
        }
    }
}
