// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public class TestCaseSkipButton : OsuTestCase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(new SkipButton(Clock.CurrentTime + 5000));
        }
    }
}
