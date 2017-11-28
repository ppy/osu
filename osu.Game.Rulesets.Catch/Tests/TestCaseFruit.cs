// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Tests.Visual;
using OpenTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestCaseFruit : OsuTestCase
    {
        public TestCaseFruit()
        {
            Add(new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        new DrawableFruit(new Fruit()) { Position = new Vector2(0.5f) },
                        new DrawableFruit(new Fruit()) { Position = new Vector2(0.5f) },
                    },
                    new Drawable[]
                    {
                        new DrawableFruit(new Fruit()) { Position = new Vector2(0.5f) },
                        new DrawableFruit(new Fruit()) { Position = new Vector2(0.5f) },
                    }
                }
            });
        }
    }
}
