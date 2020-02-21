// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneFruitObjects : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CatchHitObject),
            typeof(Fruit),
            typeof(FruitPiece),
            typeof(Droplet),
            typeof(Banana),
            typeof(BananaShower),
            typeof(DrawableCatchHitObject),
            typeof(DrawableFruit),
            typeof(DrawableDroplet),
            typeof(DrawableBanana),
            typeof(DrawableBananaShower),
            typeof(Pulp),
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (FruitVisualRepresentation rep in Enum.GetValues(typeof(FruitVisualRepresentation)))
                AddStep($"show {rep}", () => SetContents(() => createDrawable(rep)));

            AddStep("show droplet", () => SetContents(createDrawableDroplet));

            AddStep("show tiny droplet", () => SetContents(createDrawableTinyDroplet));
        }

        private Drawable createDrawableTinyDroplet()
        {
            var droplet = new TinyDroplet
            {
                StartTime = Clock.CurrentTime,
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

        private Drawable createDrawableDroplet()
        {
            var droplet = new Droplet
            {
                StartTime = Clock.CurrentTime,
                Scale = 1.5f,
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

        private Drawable createDrawable(FruitVisualRepresentation rep)
        {
            Fruit fruit = new TestCatchFruit(rep)
            {
                StartTime = 1000000000000,
                Scale = 1.5f,
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

        private class TestCatchFruit : Fruit
        {
            public TestCatchFruit(FruitVisualRepresentation rep)
            {
                VisualRepresentation = rep;
            }

            public override FruitVisualRepresentation VisualRepresentation { get; }
        }
    }
}
