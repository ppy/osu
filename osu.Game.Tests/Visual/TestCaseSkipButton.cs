// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseSkipButton : OsuTestCase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(new SkipOverlay(Clock.CurrentTime + 5000));
        }
    }
}
