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
                AddStep($"show {rep}", () => SetContents(() => createDrawable(rep)));

            AddStep("show droplet", () => SetContents(() => createDrawableDroplet()));
            AddStep("show tiny droplet", () => SetContents(createDrawableTinyDroplet));

            foreach (FruitVisualRepresentation rep in Enum.GetValues(typeof(FruitVisualRepresentation)))
                AddStep($"show hyperdash {rep}", () => SetContents(() => createDrawable(rep, true)));

            AddStep("show hyperdash droplet", () => SetContents(() => createDrawableDroplet(true)));
        }

        private Drawable createDrawableTinyDroplet()
        {
            var droplet = new TestCatchTinyDroplet
            {
                Scale = 1.5f,
            };

            return new DrawableTinyDroplet(droplet)
            {
                Anchor = Anchor.Centre,
                RelativePositionAxes = Axes.None,
                Position = Vector2.Zero,
                Alpha = 1,
                LifetimeStart = double.NegativeInfinity,
                LifetimeEnd = double.PositiveInfinity,
            };
        }

        private Drawable createDrawableDroplet(bool hyperdash = false)
        {
            var droplet = new TestCatchDroplet
            {
                Scale = 1.5f,
                HyperDashTarget = hyperdash ? new Banana() : null
            };

            return new DrawableDroplet(droplet)
            {
                Anchor = Anchor.Centre,
                RelativePositionAxes = Axes.None,
                Position = Vector2.Zero,
                Alpha = 1,
                LifetimeStart = double.NegativeInfinity,
                LifetimeEnd = double.PositiveInfinity,
            };
        }

        private Drawable createDrawable(FruitVisualRepresentation rep, bool hyperdash = false)
        {
            Fruit fruit = new TestCatchFruit(rep)
            {
                Scale = 1.5f,
                HyperDashTarget = hyperdash ? new Banana() : null
            };

            return new DrawableFruit(fruit)
            {
                Anchor = Anchor.Centre,
                RelativePositionAxes = Axes.None,
                Position = Vector2.Zero,
                Alpha = 1,
                LifetimeStart = double.NegativeInfinity,
                LifetimeEnd = double.PositiveInfinity,
            };
        }

        public class TestCatchFruit : Fruit
        {
            public TestCatchFruit(FruitVisualRepresentation rep)
            {
                VisualRepresentation = rep;
                StartTime = 1000000000000;
            }

            public override FruitVisualRepresentation VisualRepresentation { get; }
        }

        public class TestCatchDroplet : Droplet
        {
            public TestCatchDroplet()
            {
                StartTime = 1000000000000;
            }
        }

        public class TestCatchTinyDroplet : TinyDroplet
        {
            public TestCatchTinyDroplet()
            {
                StartTime = 1000000000000;
            }
        }
    }
}
