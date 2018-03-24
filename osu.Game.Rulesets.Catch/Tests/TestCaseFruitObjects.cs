// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using osu.Game.Tests.Visual;
using OpenTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestCaseFruitObjects : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CatchHitObject),
            typeof(Fruit),
            typeof(Droplet),
            typeof(DrawableCatchHitObject),
            typeof(DrawableFruit),
            typeof(DrawableDroplet),
            typeof(Pulp),
        };

        public TestCaseFruitObjects()
        {
            Add(new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        createDrawable(0),
                        createDrawable(1),
                        createDrawable(2),
                    },
                    new Drawable[]
                    {
                        createDrawable(3),
                        createDrawable(4),
                        createDrawable(5),
                    },
                }
            });
        }

        private DrawableFruit createDrawable(int index)
        {
            var fruit = new Fruit
            {
                StartTime = 1000000000000,
                IndexInBeatmap = index,
                Scale = 1.5f,
            };

            return new DrawableFruit(fruit)
            {
                Anchor = Anchor.Centre,
                RelativePositionAxes = Axes.Both,
                Position = Vector2.Zero,
                Alpha = 1,
                LifetimeStart = double.NegativeInfinity,
                LifetimeEnd = double.PositiveInfinity,
            };
        }
    }
}
