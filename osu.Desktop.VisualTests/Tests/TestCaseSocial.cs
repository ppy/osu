// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Game.Overlays;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCaseSocial : TestCase
    {
        public override string Description => @"social browser overlay";

        public override void Reset()
        {
            base.Reset();

            SocialOverlay s = new SocialOverlay();
            Add(s);

            AddStep(@"toggle", s.ToggleVisibility);
        }
    }
}
