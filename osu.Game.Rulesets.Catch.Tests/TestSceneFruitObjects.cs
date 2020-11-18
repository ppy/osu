// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneFruitObjects : CatchSkinnableTestScene
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (FruitVisualRepresentation rep in Enum.GetValues(typeof(FruitVisualRepresentation)))
                AddStep($"show {rep}", () => SetContents(() => createDrawableFruit(rep)));

            AddStep("show droplet", () => SetContents(() => createDrawableDroplet()));
            AddStep("show tiny droplet", () => SetContents(createDrawableTinyDroplet));

            foreach (FruitVisualRepresentation rep in Enum.GetValues(typeof(FruitVisualRepresentation)))
                AddStep($"show hyperdash {rep}", () => SetContents(() => createDrawableFruit(rep, true)));

            AddStep("show hyperdash droplet", () => SetContents(() => createDrawableDroplet(true)));
        }

        private Drawable createDrawableFruit(FruitVisualRepresentation rep, bool hyperdash = false) =>
            setProperties(new DrawableFruit(new TestCatchFruit(rep)), hyperdash);

        private Drawable createDrawableDroplet(bool hyperdash = false) => setProperties(new DrawableDroplet(new Droplet()), hyperdash);

        private Drawable createDrawableTinyDroplet() => setProperties(new DrawableTinyDroplet(new TinyDroplet()));

        private DrawableCatchHitObject setProperties(DrawableCatchHitObject d, bool hyperdash = false)
        {
            var hitObject = d.HitObject;
            hitObject.StartTime = 1000000000000;
            hitObject.Scale = 1.5f;

            if (hyperdash)
                hitObject.HyperDashTarget = new Banana();

            d.Anchor = Anchor.Centre;
            d.RelativePositionAxes = Axes.None;
            d.Position = Vector2.Zero;
            d.HitObjectApplied += _ =>
            {
                d.LifetimeStart = double.NegativeInfinity;
                d.LifetimeEnd = double.PositiveInfinity;
            };
            return d;
        }

        public class TestCatchFruit : Fruit
        {
            public TestCatchFruit(FruitVisualRepresentation rep)
            {
                VisualRepresentation = rep;
            }

            public override FruitVisualRepresentation VisualRepresentation { get; }
        }
    }
}
