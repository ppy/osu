// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneFruitRandomness : OsuTestScene
    {
        [Test]
        public void TestFruitRandomness()
        {
            Bindable<int> randomSeed = new Bindable<int>();

            TestDrawableFruit drawableFruit;
            TestDrawableBanana drawableBanana;

            Add(new TestDrawableCatchHitObjectSpecimen(drawableFruit = new TestDrawableFruit(new Fruit())
            {
                RandomSeed = { BindTarget = randomSeed }
            }) { X = -200 });
            Add(new TestDrawableCatchHitObjectSpecimen(drawableBanana = new TestDrawableBanana(new Banana())
            {
                RandomSeed = { BindTarget = randomSeed }
            }));

            float fruitRotation = 0;
            float bananaRotation = 0;
            float bananaScale = 0;
            Color4 bananaColour = new Color4();

            AddStep("set random seed to 0", () =>
            {
                drawableFruit.HitObject.StartTime = 500;
                randomSeed.Value = 0;

                fruitRotation = drawableFruit.InnerRotation;
                bananaRotation = drawableBanana.InnerRotation;
                bananaScale = drawableBanana.InnerScale;
                bananaColour = drawableBanana.AccentColour.Value;
            });

            AddStep("change random seed", () =>
            {
                // Use a seed value such that the banana colour is different (2/3 of the seed values are okay).
                randomSeed.Value = 10;
            });

            AddAssert("fruit rotation is changed", () => drawableFruit.InnerRotation != fruitRotation);
            AddAssert("banana rotation is changed", () => drawableBanana.InnerRotation != bananaRotation);
            AddAssert("banana scale is changed", () => drawableBanana.InnerScale != bananaScale);
            AddAssert("banana colour is changed", () => drawableBanana.AccentColour.Value != bananaColour);

            AddStep("reset random seed", () =>
            {
                randomSeed.Value = 0;
            });

            AddAssert("rotation and scale restored", () =>
                drawableFruit.InnerRotation == fruitRotation &&
                drawableBanana.InnerRotation == bananaRotation &&
                drawableBanana.InnerScale == bananaScale &&
                drawableBanana.AccentColour.Value == bananaColour);

            AddStep("change start time", () =>
            {
                drawableFruit.HitObject.StartTime = 1000;
            });

            AddAssert("random seed is changed", () => randomSeed.Value == 1000);

            AddSliderStep("random seed", 0, 100, 0, x => randomSeed.Value = x);
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
