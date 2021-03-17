// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;
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
            Vector2 bananaSize = new Vector2();
            Color4 bananaColour = new Color4();

            AddStep("Initialize start time", () =>
            {
                drawableFruit.HitObject.StartTime = drawableBanana.HitObject.StartTime = initial_start_time;

                fruitRotation = drawableFruit.DisplayRotation;
                bananaRotation = drawableBanana.DisplayRotation;
                bananaSize = drawableBanana.DisplaySize;
                bananaColour = drawableBanana.AccentColour.Value;
            });

            AddStep("change start time", () =>
            {
                drawableFruit.HitObject.StartTime = drawableBanana.HitObject.StartTime = another_start_time;
            });

            AddAssert("fruit rotation is changed", () => drawableFruit.DisplayRotation != fruitRotation);
            AddAssert("banana rotation is changed", () => drawableBanana.DisplayRotation != bananaRotation);
            AddAssert("banana size is changed", () => drawableBanana.DisplaySize != bananaSize);
            AddAssert("banana colour is changed", () => drawableBanana.AccentColour.Value != bananaColour);

            AddStep("reset start time", () =>
            {
                drawableFruit.HitObject.StartTime = drawableBanana.HitObject.StartTime = initial_start_time;
            });

            AddAssert("rotation and size restored", () =>
                drawableFruit.DisplayRotation == fruitRotation &&
                drawableBanana.DisplayRotation == bananaRotation &&
                drawableBanana.DisplaySize == bananaSize &&
                drawableBanana.AccentColour.Value == bananaColour);
        }
    }
}
