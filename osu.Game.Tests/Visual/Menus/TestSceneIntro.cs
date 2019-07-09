// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public class TestSceneIntro : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuLogo),
            typeof(StartupScreen),
            typeof(OsuScreen)
        };

        [Cached]
        private OsuLogo logo;

        public TestSceneIntro()
        {
            var rateAdjustClock = new StopwatchClock(true);
            var framedClock = new FramedClock(rateAdjustClock);
            framedClock.ProcessFrame();

            Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Clock = framedClock,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    new OsuScreenStack(new IntroCircles())
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    logo = new OsuLogo
                    {
                        Alpha = 0,
                        RelativePositionAxes = Axes.Both,
                        Position = new Vector2(0.5f),
                    },
                }
            });

            AddSliderStep("Playback speed", 0.0, 2.0, 1, v => rateAdjustClock.Rate = v);
        }
    }
}
