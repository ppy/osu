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
        private readonly TestDrawableFruit drawableFruit;
        private readonly TestDrawableBanana drawableBanana;

        public TestSceneFruitRandomness()
        {
            drawableFruit = new TestDrawableFruit(new Fruit());
            drawableBanana = new TestDrawableBanana(new Banana());

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

                fruitRotation = drawableFruit.InnerRotation;
                bananaRotation = drawableBanana.InnerRotation;
                bananaScale = drawableBanana.InnerScale;
                bananaColour = drawableBanana.AccentColour.Value;
            });

            AddStep("change start time", () =>
            {
                drawableFruit.HitObject.StartTime = drawableBanana.HitObject.StartTime = another_start_time;
            });

            AddAssert("fruit rotation is changed", () => drawableFruit.InnerRotation != fruitRotation);
            AddAssert("banana rotation is changed", () => drawableBanana.InnerRotation != bananaRotation);
            AddAssert("banana scale is changed", () => drawableBanana.InnerScale != bananaScale);
            AddAssert("banana colour is changed", () => drawableBanana.AccentColour.Value != bananaColour);

            AddStep("reset start time", () =>
            {
                drawableFruit.HitObject.StartTime = drawableBanana.HitObject.StartTime = initial_start_time;
            });

            AddAssert("rotation and scale restored", () =>
                drawableFruit.InnerRotation == fruitRotation &&
                drawableBanana.InnerRotation == bananaRotation &&
                drawableBanana.InnerScale == bananaScale &&
                drawableBanana.AccentColour.Value == bananaColour);
        }

        private class TestDrawableFruit : DrawableFruit
        {
            public float InnerRotation => ScaleContainer.Rotation;

            public TestDrawableFruit(Fruit h)
                : base(h)
            {
            }
        }

        private class TestDrawableBanana : DrawableBanana
        {
            public float InnerRotation => ScaleContainer.Rotation;
            public float InnerScale => ScaleContainer.Scale.X;

            public TestDrawableBanana(Banana h)
                : base(h)
            {
            }
        }
    }
}
