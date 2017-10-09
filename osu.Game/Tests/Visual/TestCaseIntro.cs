// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseIntro : OsuTestCase
    {
        public TestCaseIntro()
        {
            IntroSequence intro;

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
                    intro = new IntroSequence(),
                }
            });

            AddStep(@"Restart", intro.Restart);
            AddSliderStep("Playback speed", 0.0, 2.0, 1, v => rateAdjustClock.Rate = v);
        }
    }
}
