// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Tests.Visual;
using OpenTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestCaseFruitObjects : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CatchHitObject),
            typeof(Fruit),
            typeof(DrawableCatchHitObject),
            typeof(DrawableFruit),
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
                    },
                    new Drawable[]
                    {
                        createDrawable(2),
                        createDrawable(3),
                    },
                }
            });
        }

        protected override void Update()
        {
            base.Update();
        }

        private DrawableFruit createDrawable(int index) => new DrawableFruit(new Fruit
        {
            StartTime = 1000000,
            IndexInBeatmap = index
        })
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
