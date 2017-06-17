// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Game.Screens.Play;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseSkipButton : TestCase
    {
        public override string Description => @"Skip skip skippediskip";

        public override void Reset()
        {
            base.Reset();
            Add(new SkipButton(Clock.CurrentTime + 5000));
        }
    }
}
