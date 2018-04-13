// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseOsuGame : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuLogo),
        };

        public TestCaseOsuGame()
        {
            var rateAdjustClock = new StopwatchClock(true);
            var framedClock = new FramedClock(rateAdjustClock);
            framedClock.ProcessFrame();

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black,
            });

            Add(new Loader());

            AddSliderStep("Playback speed", 0.0, 2.0, 1, v => rateAdjustClock.Rate = v);
        }
    }
}
