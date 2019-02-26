// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Screens.Menu;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseOsuGame : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuLogo),
        };

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            OsuGame game = new OsuGame();
            game.SetHost(host);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                game
            };
        }
    }
}
