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
            var hit = new DrawableHit();

            AddStep("apply new hit", () => hit.Apply(PrepareObject(new Hit
            {
                Type = HitType.Rim,
                IsStrong = false,
                StartTime = 300
            })));

            AddHitObject(hit);
            RemoveHitObject(hit);

            AddStep("apply new hit", () => hit.Apply(PrepareObject(new Hit
            {
                Type = HitType.Centre,
                IsStrong = true,
                StartTime = 500
            })));

            AddHitObject(hit);
        }
    }
}
