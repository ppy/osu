// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneFruitRandomness : OsuTestScene
    {
        private readonly DrawableFruit drawableFruit;
        private readonly DrawableBanana drawableBanana;

        public TestSceneFruitRandomness()
        {
            drawableFruit = new DrawableFruit(new Fruit());
            drawableBanana = new DrawableBanana(new Banana());

            Add(new TestDrawableCatchHitObjectSpecimen(drawableFruit) { X = -200 });
            Add(new TestDrawableCatchHitObjectSpecimen(drawableBanana));

            AddSliderStep("start time", 500, 600, 0, x =>
            {
                drawableFruit.HitObject.StartTime = drawableBanana.HitObject.StartTime = x;
            });
        }

        [Test]
        public void TestFruitRandomness()
        {
            // Use values such that the banana colour changes (2/3 of the integers are okay)
            const int initial_start_time = 500;
            const int another_start_time = 501;

            float fruitRotation = 0;
            float bananaRotation = 0;
            float bananaScale = 0;
            Color4 bananaColour = new Color4();

            AddStep("Initialize start time", () =>
            {
                drawableFruit.HitObject.StartTime = drawableBanana.HitObject.StartTime = initial_start_time;

                fruitRotation = drawableFruit.Rotation;
                bananaRotation = drawableBanana.Rotation;
                bananaScale = drawableBanana.Scale.X;
                bananaColour = drawableBanana.AccentColour.Value;
            });

            AddStep("change start time", () =>
            {
                drawableFruit.HitObject.StartTime = drawableBanana.HitObject.StartTime = another_start_time;
            });

            AddAssert("fruit rotation is changed", () => drawableFruit.Rotation != fruitRotation);
            AddAssert("banana rotation is changed", () => drawableBanana.Rotation != bananaRotation);
            AddAssert("banana scale is changed", () => drawableBanana.Scale.X != bananaScale);
            AddAssert("banana colour is changed", () => drawableBanana.AccentColour.Value != bananaColour);

            AddStep("reset start time", () =>
            {
                drawableFruit.HitObject.StartTime = drawableBanana.HitObject.StartTime = initial_start_time;
            });

            AddAssert("rotation and scale restored", () =>
                drawableFruit.Rotation == fruitRotation &&
                drawableBanana.Rotation == bananaRotation &&
                drawableBanana.Scale.X == bananaScale &&
                drawableBanana.AccentColour.Value == bananaColour);
        }
    }
}
