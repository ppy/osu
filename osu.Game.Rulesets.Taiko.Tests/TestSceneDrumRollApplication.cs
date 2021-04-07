// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestSceneDrumRollApplication : HitObjectApplicationTestScene
    {
        [Test]
        public void TestApplyNewDrumRoll()
        {
            var drumRoll = new DrawableDrumRoll();

            AddStep("apply new drum roll", () => drumRoll.Apply(PrepareObject(new DrumRoll
            {
                StartTime = 300,
                Duration = 500,
                IsStrong = false,
                TickRate = 2
            }), null));

            AddHitObject(drumRoll);
            RemoveHitObject(drumRoll);

            AddStep("apply new drum roll", () => drumRoll.Apply(PrepareObject(new DrumRoll
            {
                StartTime = 150,
                Duration = 400,
                IsStrong = true,
                TickRate = 16
            }), null));

            AddHitObject(drumRoll);
        }
    }
}
