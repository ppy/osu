// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModDifficultyAdjust : ModTestScene
    {
        public TestSceneOsuModDifficultyAdjust()
            : base(new OsuRuleset())
        {
        }

        [Test]
        public void TestNoAdjustment() => CreateModTest(new ModTestData
        {
            Mod = new OsuModDifficultyAdjust(),
            Autoplay = true,
            PassCondition = checkSomeHit
        });

        [Test]
        public void TestCircleSize1() => CreateModTest(new ModTestData
        {
            Mod = new OsuModDifficultyAdjust { CircleSize = { Value = 1 } },
            Autoplay = true,
            PassCondition = () => checkSomeHit() && checkObjectsScale(0.78f)
        });

        [Test]
        public void TestCircleSize10() => CreateModTest(new ModTestData
        {
            Mod = new OsuModDifficultyAdjust { CircleSize = { Value = 10 } },
            Autoplay = true,
            PassCondition = () => checkSomeHit() && checkObjectsScale(0.15f)
        });

        [Test]
        public void TestApproachRate1() => CreateModTest(new ModTestData
        {
            Mod = new OsuModDifficultyAdjust { ApproachRate = { Value = 1 } },
            Autoplay = true,
            PassCondition = () => checkSomeHit() && checkObjectsPreempt(1680)
        });

        [Test]
        public void TestApproachRate10() => CreateModTest(new ModTestData
        {
            Mod = new OsuModDifficultyAdjust { ApproachRate = { Value = 10 } },
            Autoplay = true,
            PassCondition = () => checkSomeHit() && checkObjectsPreempt(450)
        });

        private bool checkObjectsPreempt(double target)
        {
            var objects = Player.ChildrenOfType<DrawableHitCircle>();
            if (!objects.Any())
                return false;

            return objects.All(o => o.HitObject.TimePreempt == target);
        }

        private bool checkObjectsScale(float target)
        {
            var objects = Player.ChildrenOfType<DrawableHitCircle>();
            if (!objects.Any())
                return false;

            return objects.All(o => Precision.AlmostEquals(o.ChildrenOfType<ShakeContainer>().First().Children.OfType<Container>().Single().Scale.X, target));
        }

        private bool checkSomeHit()
        {
            return Player.ScoreProcessor.JudgedHits >= 2;
        }
    }
}
