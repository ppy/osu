// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneHitApplication : HitObjectApplicationTestScene
    {
        [Test]
        public void TestApplyNewHit()
        {
            // Now we have pools for different types:
            // ```
            // RegisterPool<HitRim, DrawableHitRim>(50);
            // RegisterPool<HitCentre, DrawableHitCentre>(50);
            // ```
            // And therefore we cant Apply between differ HitTypes

            var first_rim = new HitRim
            {
                IsStrong = false,
                StartTime = 100
            };
            var hit_rim = DrawableHit.CreateConcrete(first_rim);

            AddStep("apply new rim hit", () => hit_rim.Apply(PrepareObject(new HitRim
            {
                IsStrong = false,
                StartTime = 300
            })));

            AddHitObject(hit_rim);
            RemoveHitObject(hit_rim);

            var first_centre = new HitCentre
            {
                IsStrong = false,
                StartTime = 400
            };
            var hit_centre = DrawableHit.CreateConcrete(first_centre);

            AddStep("apply new centre hit", () => hit_centre.Apply(PrepareObject(new HitCentre
            {
                IsStrong = true,
                StartTime = 500
            })));

            AddHitObject(hit_centre);
        }
    }
}
